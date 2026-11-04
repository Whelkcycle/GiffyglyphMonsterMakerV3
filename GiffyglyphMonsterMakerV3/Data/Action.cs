using System.ComponentModel;
using GiffyglyphMonsterMakerV3.Utility;
using Ganss.Xss;
using Microsoft.AspNetCore.Components;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace GiffyglyphMonsterMakerV3.Data
{
    public class Action : Feature
    {
        public Action(string createUserId) : base(createUserId)
        {
            Name = "New Action";
        }
        public override string MarkupDescription(Creature parentCreature)
        {
            string desc =
                "<span class=\"text-white fa-solid p-1 align-middle " + (String.IsNullOrWhiteSpace(CustomIcon) ? Icon : CustomIcon) + " " + RarityStyle + "\"></span></span><span class=\"ms-1\">";

            desc += @"<span class=""fw-bold"">" + Name;
            if (!string.IsNullOrWhiteSpace(FrequencyDisplayText))
            {
                desc += " (" + FrequencyDisplayText + ")";
            }
            desc += @": </span>";
            if (Concentration)
            {
                desc += "<span class=\"fst-italic\">Requires Concentration </span>";
            }
            //If you want to just totally override a thing, go for it
            if (!string.IsNullOrWhiteSpace(OverrideMarkup))
            {
                var sanitizer = new HtmlSanitizer();
                (new List<string> { "fst-italic", "fw-bold" }).ForEach(item => sanitizer.AllowedClasses.Add(item));
                var html = OverrideMarkup;
                return desc + sanitizer.Sanitize(html);
            }

            desc += (IsSpell ? "<span class=\"fst-italic\">Spell</span>: " + SpellDesc + " " : "");
            if ((IsAttack || IsSpell) && Shape != TargetShape.self)
                desc += "<span class=\"fst-italic\">" + Distance.ToString() + "</span>: ";

            string shapeText = "";
            switch (Shape)
            {
                case TargetShape.line:
                    shapeText += "a " + Range + " ft. line extending from yourself.";
                    break;
                case TargetShape.cone:
                    shapeText += "a " + Range + " ft. cone extending from yourself.";
                    break;
                case TargetShape.emanation:
                    shapeText += "Targets within " + Range + " ft. of yourself.";
                    break;
                case TargetShape.circle:
                    shapeText += "a " + Radius + " ft. circle centered within " + Range + " ft.";
                    break;
                case TargetShape.cube:
                    shapeText += "a " + Radius + " ft. cube centered within " + Range + " ft.";
                    break;
                case TargetShape.square:
                    shapeText += "a " + Radius + " ft. square centered within " + Range + " ft.";
                    break;
                case TargetShape.sphere:
                    shapeText += "a " + Radius + " ft. sphere centered within " + Range + " ft.";
                    break;
                case TargetShape.self:
                    shapeText += "";
                    break;
                case TargetShape.wall:
                    shapeText += "a " + Radius + " ft. long wall, 5 ft. wide, with its midpoint within " + Range +
                                 " ft.";
                    break;
                case TargetShape.target:
                default:
                    shapeText += Targets + " target";
                    if (Targets != 1)
                        shapeText += "s";

                    shapeText += ".";
                    break;
            }

            if (HasSave)
            {
                desc += "DC" + (parentCreature.Offense.DifficultyCheck + parentCreature.Attributes.Dict[RelevantAttribute] + parentCreature.Attributes.AttributeMod) + " vs " + SaveVs + ", ";
            }

            if (IsAttack)
            {
                desc += ((parentCreature.Offense.Attack + parentCreature.Attributes.Dict[RelevantAttribute] + parentCreature.Attributes.AttributeMod) >= 0 ? "+" : "") + (parentCreature.Offense.Attack + parentCreature.Attributes.Dict[RelevantAttribute] + parentCreature.Attributes.AttributeMod) + " to hit, ";
            }

            if (Shape == TargetShape.target)
            {
                switch (Distance)
                {
                    case RangeType.Melee:
                        desc += "reach ";
                        desc += Range + " ft.,";
                        break;
                    case RangeType.Ranged:
                    default:
                        desc += "range " + Range + " ft.,";
                        break;
                }
            }



            if (IsAttack && MultiAttack > 1)
            {
                desc += " " + MultiAttack + " attacks, ";
            }
            desc += " " + shapeText;

            if (DealsDamage || IsAttack)
            {
                desc += " <span class=\"fst-italic\">Hit</span>: ";
            }

            if (DealsDamage)
            {
                var dam = (int)Math.Max(Math.Floor((double)(parentCreature.Offense.Damage * DamageMultiplier) / MultiAttack), 1);
                if (OverrideRandomDamage)
                {
                    desc += dam + " (" + DiceTools.ConvertToDiceString(OverrideRandomDamageRange, dam) + ")" + " " + ActionDamageType.ToDescriptionString() +
                            " damage.";
                }
                else
                {
                    desc += dam + (parentCreature.Offense.RandomizeDamage ? " (" + DiceTools.ConvertToDiceString(parentCreature.Offense.RandomDamageRange, dam) + ")" : "") + " " + ActionDamageType.ToDescriptionString() +
                            " damage.";
                }
            }

            if (!String.IsNullOrWhiteSpace(OtherEffect))
                desc += " " + OtherEffect;
            if (!String.IsNullOrWhiteSpace(MissEffect))
            {
                desc += " <span class=\"fst-italic\">Miss</span>: ";
                desc += " " + MissEffect;
            }

            desc += "</span>";
            return desc;

        }

        public override string Icon
        {
            get
            {
                if (IsSpell)
                {
                    return "fa-wand-magic-sparkles";
                }
                
                if (IsAttack)
                {
                    switch (Distance)
                    {
                        case RangeType.Melee:
                            return "fa-sword";
                        case RangeType.Ranged:
                            return "fa-bow-arrow";
                        default:
                            throw new ArgumentOutOfRangeException();
                    }
                }

                return "fa-hand-back-fist";


            }
            }
        public override FeatureType Type { get; set; } = FeatureType.Action;
        public RangeType Distance { get; set; }
        public int Range { get; set; } = 5;
        public int Radius { get; set; }
        public bool IsAttack { get; set; }
        public bool IsSpell { get; set; }
        public string SpellDesc { get; set; } = "";
        public bool DealsDamage { get; set; } = true;
        public int MultiAttack { get; set; } = 1;
        public bool Concentration { get; set; }

        [NotMapped]
        public bool IsMultiAttack
        {
            get => MultiAttack != 1;
            set => MultiAttack = value ? 2 : 1;
        }
        public double DamageMultiplier { get; set; } = 1;
        public string OtherEffect { get; set; } = "";
        public string MissEffect { get; set; } = "";
        public DamageType ActionDamageType { get; set; } = DamageType.bludgeoning;
        public int Targets { get; set; } = 1;
        public TargetShape Shape { get; set; } = TargetShape.target;
        public Role? AssociatedRole { get; set; }
        public bool OverrideRandomDamage { get; set;} = false;
        public DamageRange OverrideRandomDamageRange { get; set; }
        public override void UpdateThisToMatch(Object o)
        {
            if (o is not Action a)
                throw new InvalidDataException("Target is not an action");

            Distance = a.Distance;
            Range = a.Range;
            Radius = a.Radius;
            IsSpell = a.IsSpell;
            SpellDesc = a.SpellDesc;
            DealsDamage = a.DealsDamage;
            MultiAttack = a.MultiAttack;
            DamageMultiplier = a.DamageMultiplier;
            OtherEffect = a.OtherEffect;
            MissEffect = a.MissEffect;
            ActionDamageType = a.ActionDamageType;
            Targets = a.Targets;
            Shape = a.Shape;
            IsAttack = a.IsAttack;
            AssociatedRole = a.AssociatedRole;
            Concentration = a.Concentration;
            OverrideRandomDamage = a.OverrideRandomDamage;
            OverrideRandomDamageRange = a.OverrideRandomDamageRange;

            base.UpdateThisToMatch(o);
        }
    }
    public enum DamageType
    {
        acid = 0,
        bludgeoning,
        cold,
        fire,
        force,
        lightning,
        necrotic,
        piercing,
        poison,
        psychic,
        radiant,
        slashing,
        thunder,
        healing,
        [Description("temporary healing")]
        temporary_healing
    }
    public enum TargetShape
    {
        line = 0,
        cone,
        emanation,
        circle,
        target,
        wall,
        square,
        cube,
        sphere,
        self
    }
    public enum RangeType
    {
        Melee = 0,
        Ranged
    }
}
