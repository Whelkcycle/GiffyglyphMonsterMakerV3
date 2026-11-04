using System.Security.Claims;
using System.Text.Json;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.EntityFrameworkCore;

namespace GiffyglyphMonsterMakerV3.Data
{
    public class CreatureTemplateService
    {
        private readonly IDbContextFactory<ApplicationDbContext> _dbContextFactory;
        private readonly AuthenticationStateProvider _authenticationStateProvider;
        private static readonly JsonSerializerOptions JsonOptions = new()
        {
            PropertyNameCaseInsensitive = true
        };

        public CreatureTemplateService(
            IDbContextFactory<ApplicationDbContext> dbContextFactory,
            AuthenticationStateProvider authenticationStateProvider)
        {
            _dbContextFactory = dbContextFactory;
            _authenticationStateProvider = authenticationStateProvider;
        }

        private async Task<string> CurrentUserIdAsync()
        {
            var state = await _authenticationStateProvider.GetAuthenticationStateAsync();
            return state.User.FindFirstValue(ClaimTypes.NameIdentifier)
                ?? throw new InvalidOperationException("A signed-in user is required.");
        }

        public async Task<List<CreatureTemplate>> GetAllAsync()
        {
            var userId = await CurrentUserIdAsync();
            await using var db = await _dbContextFactory.CreateDbContextAsync();
            return await db.CreatureTemplates.Where(t => t.CreateUserId == userId)
                .OrderBy(t => t.Layer).ThenBy(t => t.Name).ToListAsync();
        }

        public async Task<bool> SaveAsync(CreatureTemplate template)
        {
            var userId = await CurrentUserIdAsync();
            await using var db = await _dbContextFactory.CreateDbContextAsync();

            if (template.ParentTemplateId.HasValue)
            {
                var parent = await db.CreatureTemplates.FirstOrDefaultAsync(t =>
                    t.Id == template.ParentTemplateId.Value && t.CreateUserId == userId);
                if (parent == null || (int)parent.Layer != (int)template.Layer - 1)
                    throw new InvalidOperationException("A Family must inherit from a Type, and a Genus must inherit from a Family.");
            }
            else if (template.Layer != CreatureTemplateLayer.Type)
            {
                throw new InvalidOperationException("Family and Genus templates require a parent template.");
            }

            if (template.Id == Guid.Empty)
                template.Id = Guid.NewGuid();
            template.CreateUserId = userId;
            template.Name = template.Name.Trim();
            if (string.IsNullOrWhiteSpace(template.Name))
                throw new InvalidOperationException("Template name is required.");

            var existing = await db.CreatureTemplates.FirstOrDefaultAsync(t =>
                t.Id == template.Id && t.CreateUserId == userId);
            if (existing == null)
                await db.CreatureTemplates.AddAsync(template);
            else
            {
                existing.Name = template.Name;
                existing.Layer = template.Layer;
                existing.ParentTemplateId = template.ParentTemplateId;
                existing.PayloadJson = template.PayloadJson;
            }
            await db.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeleteAsync(Guid id)
        {
            var userId = await CurrentUserIdAsync();
            await using var db = await _dbContextFactory.CreateDbContextAsync();
            var template = await db.CreatureTemplates.FirstOrDefaultAsync(t => t.Id == id && t.CreateUserId == userId);
            if (template == null) return false;
            if (await db.CreatureTemplates.AnyAsync(t => t.ParentTemplateId == id && t.CreateUserId == userId))
                throw new InvalidOperationException("This template has child templates. Reassign or delete them first.");
            db.CreatureTemplates.Remove(template);
            await db.SaveChangesAsync();
            return true;
        }

        public async Task ApplyAsync(Monster monster, Guid? typeId, Guid? familyId, Guid? genusId)
        {
            var userId = await CurrentUserIdAsync();
            await using var db = await _dbContextFactory.CreateDbContextAsync();
            var all = await db.CreatureTemplates.Where(t => t.CreateUserId == userId).ToListAsync();
            var chosen = new[] { typeId, familyId, genusId };
            var expected = new[] { CreatureTemplateLayer.Type, CreatureTemplateLayer.Family, CreatureTemplateLayer.Genus };
            var chain = new List<CreatureTemplate>();

            for (var i = 0; i < chosen.Length; i++)
            {
                if (!chosen[i].HasValue)
                {
                    if (chosen.Skip(i + 1).Any(v => v.HasValue))
                        throw new InvalidOperationException("Select templates in order: Type, then optional Family, then optional Genus.");
                    continue;
                }
                if (i > 0 && !chosen[i - 1].HasValue)
                    throw new InvalidOperationException("A child template requires its parent template.");
                var item = all.FirstOrDefault(t => t.Id == chosen[i]!.Value);
                if (item == null || item.Layer != expected[i])
                    throw new InvalidOperationException("The selected templates do not match the Type → Family → Genus hierarchy.");
                if (i > 0 && item.ParentTemplateId != chain[i - 1].Id)
                    throw new InvalidOperationException("Each child template must inherit from the selected parent.");
                chain.Add(item);
            }

            if (chain.Count == 0) return;
            if (chain[0].Layer != CreatureTemplateLayer.Type)
                throw new InvalidOperationException("Select a Type template first.");

            foreach (var template in chain)
            {
                var payload = JsonSerializer.Deserialize<CreatureTemplatePayload>(template.PayloadJson, JsonOptions)
                    ?? new CreatureTemplatePayload();
                ApplyPayload(monster, payload, userId);
            }
        }

        private static void ApplyPayload(Monster monster, CreatureTemplatePayload payload, string userId)
        {
            if (payload.Size.HasValue) monster.Size = payload.Size.Value;
            if (payload.CreatureType.HasValue) monster.Type = payload.CreatureType.Value;
            if (payload.TypeDetail != null) monster.TypeDetail = payload.TypeDetail;
            if (payload.WalkSpeed.HasValue) monster.WalkSpeed = payload.WalkSpeed.Value;

            ApplyEnumChanges<MovementType>(payload.Speeds, monster.OtherSpeeds);
            ApplyEnumChanges<SenseType>(payload.Senses, monster.Senses);
            ApplyListChanges(payload.Resistances, monster.Defenses.Resistances);
            ApplyListChanges(payload.Immunities, monster.Defenses.Immunities);
            ApplyListChanges(payload.Vulnerabilities, monster.Defenses.Vulnerabilities);
            ApplyListChanges(payload.Skills, monster.Skills);

            foreach (var change in payload.Traits)
            {
                var existing = monster.Features.OfType<Trait>()
                    .FirstOrDefault(t => string.Equals(t.Name, change.Name, StringComparison.OrdinalIgnoreCase));
                if (change.Operation == TemplateChangeOperation.Remove)
                {
                    if (existing != null) monster.Features.Remove(existing);
                }
                else if (change.Operation == TemplateChangeOperation.Modify)
                {
                    if (existing != null)
                    {
                        existing.TraitDescription = change.Description;
                        existing.OverrideMarkup = "";
                        existing.FrequencyText = change.FrequencyText;
                    }
                }
                else if (existing == null && !string.IsNullOrWhiteSpace(change.Name))
                {
                    monster.Features.Add(new Trait(userId)
                    {
                        Name = change.Name.Trim(),
                        TraitDescription = change.Description,
                        FrequencyText = change.FrequencyText,
                        Rarity = RarityType.Common,
                        ParentId = monster.Id
                    });
                }
            }
        }

        private static void ApplyListChanges(List<TemplateValueChange> changes, List<string> target)
        {
            foreach (var change in changes)
            {
                var existing = target.FirstOrDefault(v => string.Equals(v, change.Value, StringComparison.OrdinalIgnoreCase));
                if (change.Operation == TemplateChangeOperation.Remove)
                {
                    if (existing != null) target.Remove(existing);
                }
                else if (change.Operation == TemplateChangeOperation.Add && existing == null && !string.IsNullOrWhiteSpace(change.Value))
                    target.Add(change.Value.Trim());
            }
        }

        private static void ApplyEnumChanges<TEnum>(List<TemplateValueChange> changes, Dictionary<TEnum, int> target)
            where TEnum : struct, Enum
        {
            foreach (var change in changes)
            {
                var parts = change.Value.Split(':', 2, StringSplitOptions.TrimEntries);
                if (!Enum.TryParse<TEnum>(parts[0], true, out var key)) continue;
                if (change.Operation == TemplateChangeOperation.Remove)
                    target.Remove(key);
                else if (change.Operation == TemplateChangeOperation.Add && parts.Length == 2 &&
                         int.TryParse(parts[1], out var value))
                    target[key] = value;
                else if (change.Operation == TemplateChangeOperation.Modify && parts.Length == 2 &&
                         int.TryParse(parts[1], out var value))
                    target[key] = value;
            }
        }
    }
}
