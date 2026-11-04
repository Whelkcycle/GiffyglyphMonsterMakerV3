using System.ComponentModel.DataAnnotations.Schema;
using System.Runtime.InteropServices;
using Ganss.Xss;

namespace GiffyglyphMonsterMakerV3.Data
{
    public class Trait : Feature
    {
        public Trait(string createUserId) : base(createUserId)
        {
            Name = "New Trait";
            Type = FeatureType.Trait;
        }
        public override string MarkupDescription(Creature parentCreature)
        {
            string desc =
                "<span class=\"text-white fas p-1 align-middle " + (String.IsNullOrWhiteSpace(CustomIcon) ? Icon : CustomIcon) + " " + RarityStyle + "\"></span></span><span class=\"ms-1\">";
            desc += @"<span class=""fw-bold"">" + Name;
            if (!string.IsNullOrWhiteSpace(FrequencyDisplayText))
                desc += " (" + FrequencyDisplayText + ")";
            desc += ":</span> ";
            //If you want to just totally override a thing, go for it
            if (!string.IsNullOrWhiteSpace(OverrideMarkup))
            {
                var sanitizer = new HtmlSanitizer();
                (new List<string> { "fst-italic", "fw-bold" }).ForEach(item => sanitizer.AllowedClasses.Add(item));
                var html = OverrideMarkup;
                return desc + sanitizer.Sanitize(html);
            }

            desc += TraitDescription;
            
            return desc;
        }
        public override void UpdateThisToMatch(Object o)
        {
            if (o is not Trait a)
                throw new InvalidDataException("Target is not a Trait");

            TraitDescription = ((Trait)o).TraitDescription;
            AssociatedCreatureType = ((Trait)o).AssociatedCreatureType;

            base.UpdateThisToMatch(o);
        }
        public override string Icon
        {
            get
            {
                return "fa-paw-claws";
            }
        }
        public string TraitDescription { get; set; }
        public CreatureType? AssociatedCreatureType { get; set; }
    }
}
