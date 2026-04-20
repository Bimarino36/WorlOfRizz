# MVP Acceptance Checklist

## Restaurant_Main
- [ ] `Restaurant_Main` launches as the primary gameplay scene.
- [ ] Guests complete the full loop: spawn -> seat -> order -> eat -> bill -> exit.
- [ ] Queue, urgency, patience, and walkouts still behave correctly.
- [ ] Upgrades, save/load, and offline income still work.
- [ ] Kitchen, bar, cleaner, and manager presentation remain visual-only helpers.
- [ ] Meta HUD shows Rare, Seeds, Ingredients, and restaurant contracts.
- [ ] Pending restaurant coins from other scenes are auto-applied on restaurant entry.

## Adventure_World
- [ ] Scene loads from the restaurant portal and returns to the restaurant cleanly.
- [ ] Auto-attack, one active skill, wave flow, and boss encounter complete without errors.
- [ ] Reward screen appears on win and loss.
- [ ] Rewards persist into meta save after scene exit and restart.

## Farm_Garden
- [ ] Scene loads from the restaurant portal and returns to the restaurant cleanly.
- [ ] Plots support plant -> grow -> ready -> harvest loop.
- [ ] Seeds are consumed and ingredients are added correctly.
- [ ] Farm orders cannot spend resources below zero.
- [ ] Farm rewards persist into meta save after scene exit and restart.

## Meta Loop
- [ ] Adventure rewards add Rare, Seeds, and pending restaurant coins.
- [ ] Farm consumes Seeds and produces Ingredients.
- [ ] Restaurant contracts consume meta resources and grant restaurant rewards safely.
- [ ] Restaurant, Adventure, and Farm remain unlocked in the current MVP flow.
- [ ] Missing or empty meta save does not break startup.

## Stability
- [ ] Solution builds without errors.
- [ ] `Restaurant_Main`, `Adventure_World`, and `Farm_Garden` validate without broken references.
- [ ] Build settings contain the three MVP scenes in the intended order.
- [ ] No temporary scene or fallback scene is re-enabled in build settings by accident.
