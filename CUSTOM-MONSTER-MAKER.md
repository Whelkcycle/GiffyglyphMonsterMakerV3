# Custom Monster Maker changes

This branch adds three customization layers while retaining the original Monster Maker level/role calculations.

## Before first launch

The application uses SQL Server. Back up the database, then run the idempotent upgrade script once:

`GiffyglyphMonsterMakerV3/Database/upgrade-free-text-frequency-and-feature-order.sql`

The script adds free-text feature frequency and sort-order columns, the creature skills field, and the `CreatureTemplates` table. It preserves the old structured frequency columns so existing saved records remain readable.

## Feature frequency

The feature editor now has one free-text **Frequency / Recharge** field. Enter the exact text that should appear in the statblock, for example:

- `Recharge 5–6`
- `Recharge 6`
- `Once per encounter`
- `Three times per day`

The application does not parse, validate, or reinterpret this text. Leaving it blank uses the legacy frequency value for older saved features. Feature order can be changed with the up/down arrows beside each feature.

## Creature templates

The Creature Templates panel is at the top of the Monster Maker page.

1. Create a **Type** template.
2. Create an optional **Family** template and choose its Type parent.
3. Create an optional **Genus** template and choose its Family parent.
4. Select the Type, then its optional Family and Genus, and apply them to the current monster.
5. Set Combat Level and Role in the existing Monster Details section. Existing level/role calculations remain unchanged.

Each template stores only the changes authored in that layer. Applying a chain processes Type, Family, then Genus. A child can add, remove, or modify a trait inherited from its parent by matching the trait name (case-insensitively). Editing a template definition does not edit its parent or sibling templates.

### Template change syntax

For list changes (resistances, immunities, vulnerabilities, skills), enter one operation per line:

- `+fire` — add a value
- `-cold` — remove a value

For movement and senses, include the numeric value after a colon:

- `+Fly:60` — add or set flying speed
- `~Climb:40` — change climbing speed
- `-Swim` — remove swimming speed
- `+darkvision:60` — add darkvision
- `-blindsight` — remove blindsight

For traits, use one operation per line:

- `+Titanfall | The creature crashes down with tremendous force. | ` — add a trait
- `~Regeneration | Radiant damage suppresses this trait. | ` — modify an inherited trait
- `-Titanfall` — remove an inherited trait

Trait names are the matching key for modify/remove operations. The frequency segment is optional and is displayed verbatim.

## Skills

Skills are editable in the Monster Maker's Abilities section and can be added or removed by templates. Skill names are stored as text so custom skills are also possible.

## Build validation

A GitHub Actions workflow has been added to restore and build the .NET 9 project on pushes to this branch. The code changes should be treated as unverified until that workflow reports a successful build.
