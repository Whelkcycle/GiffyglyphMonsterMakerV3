using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics.CodeAnalysis;
using GiffyglyphMonsterMakerV3.Utility;
using Microsoft.EntityFrameworkCore.Metadata.Conventions;
using Ganss.Xss;
using System.Security.Claims;

namespace GiffyglyphMonsterMakerV3.Data
{
    [Table("Features")]
    public class Feature
    {

        [Key]
        public Guid Id { get; set; }
        public Guid? TemplateId { get; set; }
        [ForeignKey("Id")]
        public string CreateUserId { get; set; }
        public string Name { get; set; } = "";

        public virtual string MarkupDescription(Creature parentCreature)
        {
            string desc =
                "<span class=\"text-white fa-solid p-1 align-middle " + (String.IsNullOrWhiteSpace(CustomIcon) ? Icon : CustomIcon) + " " + RarityStyle + "\"></span></span><span class=\"ms-1\">";

            desc += @"<span class=""fw-bold"">" + Name;
            if (!string.IsNullOrWhiteSpace(FrequencyDisplayText))
            {
                desc += " (" + FrequencyDisplayText + ")";
            }
            desc += @": </span>";

            return desc + _sanitizer.Sanitize(OverrideMarkup);
        }

        [NotMapped] private HtmlSanitizer _sanitizer;

        public Feature(string createUserId)
        {
            _sanitizer = new HtmlSanitizer();
            (new List<string> { "fst-italic", "fw-bold" }).ForEach(item => _sanitizer.AllowedClasses.Add(item));
            CreateUserId = createUserId;
        }

        private string _overrideMarkup = "";
        [BackingField("_overrideMarkup")]
        public string OverrideMarkup
        {
            get => _overrideMarkup;
            set
            {
                var html = value;
                _overrideMarkup = _sanitizer.Sanitize(html);
            }
        }

        public virtual FeatureType Type { get; set; }
        public RarityType Rarity { get; set; }
        public AttributeType RelevantAttribute { get; set; }

        public virtual string Icon
        {
            get
            {
                return "fa-play";
            }
        }

        public string CustomIcon { get; set; } = "";
        public virtual string RarityStyle
        {
            get
            {
                string style = "";
                switch (Rarity)
                {
                    case RarityType.Common:
                        style += "ability-common rounded-circle";
                        break;
                    case RarityType.Uncommon:
                        style += "ability-uncommon rounded-end rounded-start-bottom";
                        break;
                    case RarityType.Rare:
                        style += "ability-rare rounded-end-top rounded-start-bottom";
                        break;
                    default:
                        style += "";
                        break;
                }
                return style;
            }
        }
        public bool HasSave { get; set; }
        public string SaveVs { get; set; } = "";
        // Free-text frequency replaces the old structured cooldown/delay controls.
        // The legacy object remains mapped so existing databases and saved monsters remain readable.
        public string FrequencyText { get; set; } = "";
        [NotMapped]
        public string FrequencyDisplayText => !string.IsNullOrWhiteSpace(FrequencyText)
            ? FrequencyText
            : (Frequency != null && (Frequency.Type != FrequencyType.passive || Frequency.Delay != DelayType.none) ? Frequency.StringValue : "");
        public FeatureFrequency Frequency { get; set; } = new();
        public int SortOrder { get; set; }
        [ForeignKey("Id")]
        public Guid? ParentId { get; set; }

        public virtual void UpdateThisToMatch(object o)
        {
            if (o is not Feature f)
                throw new InvalidDataException("Target is not a feature");

            Name = f.Name;
            OverrideMarkup = f.OverrideMarkup;
            Rarity = f.Rarity;
            RelevantAttribute = f.RelevantAttribute;
            CustomIcon = f.CustomIcon;
            HasSave = f.HasSave;
            SaveVs = f.SaveVs;
            FrequencyText = f.FrequencyText;
            SortOrder = f.SortOrder;
            Guid tempFreqId = Frequency.Id;
            Frequency = f.Frequency.Clone();
            Frequency.Id = tempFreqId;
        }
    }
    public enum FeatureType
    {
        Trait = 0,
        Free,
        Action,
        Bonus,
        Reaction,
        Countermeasure
    }
    public enum RarityType
    {
        Common = 0,
        Uncommon,
        Rare
    }

    public enum FrequencyType
    {
        passive = 0,
        cooldown,
        shortrest,
        longrest,
        charge,
        round,
        recharge
    }

    public enum DelayType
    {
        none = 0,
        delay,
        doom
    }
    //Mostly unused except for the immuinities dropdown
    //These are the 5e default ones
    public enum ConditionType
    {
        blinded,
        charmed,
        deafened,
        frightened,
        grappled,
        incapacitated,
        invisible,
        paralyzed,
        petrified,
        poisoned,
        prone,
        restrained,
        stunned,
        unconscious,
        exhaustion
    }
    public class FeatureFrequency
    {
        [Key]
        public Guid Id { get; set; }
        public FrequencyType Type { get; set; }
        public DelayType Delay { get; set; }
        public int Value { get; set; } = 0;
        public int DelayValue { get; set; } = 0;

        public string StringValue
        {
            get
            {
                string desc = "";
                switch (Type)
                {
                    case FrequencyType.passive:
                        break;
                    case FrequencyType.cooldown:
                        desc += "Cooldown " + Value;
                        break;
                    case FrequencyType.shortrest:
                        desc += Value + "/sr";
                        break;
                    case FrequencyType.longrest:
                        desc += Value + "/lr";
                        break;
                    case FrequencyType.charge:
                        desc += Value + " charge" + (Value != 1 ? "s" : "");
                        break;
                    case FrequencyType.round:
                        desc += Value + "/round";
                        break;
                    case FrequencyType.recharge:
                        desc += "Recharge " + Value;
                        break;
                }

                if (!string.IsNullOrWhiteSpace(desc) && Delay != DelayType.none) desc += ", ";

                switch (Delay)
                {
                    case DelayType.delay:
                        desc += "Delayed " + DelayValue;
                        break;
                    case DelayType.doom:
                        desc += "Dooming " + DelayValue;
                        break;
                }

                return desc;
            }
        }
    }
}
