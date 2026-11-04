using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GiffyglyphMonsterMakerV3.Data
{
    public enum CreatureTemplateLayer
    {
        Type = 0,
        Family = 1,
        Genus = 2
    }

    public enum TemplateChangeOperation
    {
        Add = 0,
        Remove = 1,
        Modify = 2
    }

    public class CreatureTemplate
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();
        [Required]
        public string CreateUserId { get; set; } = "";
        [Required]
        public string Name { get; set; } = "";
        public CreatureTemplateLayer Layer { get; set; }
        public Guid? ParentTemplateId { get; set; }
        [Required]
        public string PayloadJson { get; set; } = "{}";
    }

    // Only explicitly supplied values are applied. Null means "inherit from parent".
    public class CreatureTemplatePayload
    {
        public SizeType? Size { get; set; }
        public CreatureType? CreatureType { get; set; }
        public string? TypeDetail { get; set; }
        public int? WalkSpeed { get; set; }
        public List<TemplateValueChange> Speeds { get; set; } = new();
        public List<TemplateValueChange> Senses { get; set; } = new();
        public List<TemplateValueChange> Resistances { get; set; } = new();
        public List<TemplateValueChange> Immunities { get; set; } = new();
        public List<TemplateValueChange> Vulnerabilities { get; set; } = new();
        public List<TemplateValueChange> Skills { get; set; } = new();
        public List<TemplateTraitChange> Traits { get; set; } = new();
    }

    public class TemplateValueChange
    {
        public TemplateChangeOperation Operation { get; set; } = TemplateChangeOperation.Add;
        public string Value { get; set; } = "";
    }

    public class TemplateTraitChange
    {
        public TemplateChangeOperation Operation { get; set; } = TemplateChangeOperation.Add;
        public string Name { get; set; } = "";
        public string Description { get; set; } = "";
        public string FrequencyText { get; set; } = "";
    }
}
