# Changelog

<details>
<summary>1.3.2</summary>

## Changed
- [(#2)](https://github.com/QuadMesh/QMLC_YippeeKeyMod/issues/2) Updated Csync dependancy to 2.0.0, fixing a game-breaking issue in the process.
- [(#3)](https://github.com/QuadMesh/QMLC_YippeeKeyMod/issues/3) Fixed an issue that could occur when the Company building kills the player, that would cause the mod to stop working completely.
</details>
<hr>

<details>
<summary>1.3.1</summary>

## Changed
- Fixed an issue that could occur when dying by certain events (Dead body being destroyed)
</details>
<hr>

<details>
<summary>1.3.0</summary>

## Added
- local pitch fluctuation to make shouting 'Yippee!' more fun.
- Dead players may now be heard by alive players (very silently)
- Config that dictates whether or not dead players can yippe to alive players [Default: on]
- Config that dictates whether or not dead players alert Enemies [Default: off]
- Config that allows 'Yippee!'s to fluctuate in pitch when shouted. [Default: on]
- Config that allows 'Yippee!'s to overlap instead of restarting [Default: off]
- Added more logging entries for these events.

## Changed
- Updated Csync Dependency from 1.0.7 to 1.0.8
- Confetti now scales down over time, looks really nice!
- Confetti now spawns in a circle around the player using noise to add a more 'weightless' feeling.
- Reworked a majority of the networking code.
- Certain config names changed

</details>
<hr>

<details>
<summary>1.2.2</summary>

## Changed

- Patched an issue where using chat could cause an unexpected 'Yippee!' to be shouted. [(Issue #1)](https://github.com/QuadMesh/QMLC_YippeeKeyMod/issues/1)
- Patched an issue where stray confetti was spawned when dead players used 'Yippee!

</details>
<hr>
<details>
<summary>1.2.1</summary>

## Changed

- Updated Csync Dependency from 1.0.6 to 1.0.7
- Updated changelog file to fix spelling mistakes

</details>
<hr>
<details>
<summary>1.2.0</summary>

## Added

- Dead players can now 'Yippee' to eachother!
- Cooldown config for those who dislike spamming. Please note: this is Host controlled, make sure to ask the host to enable cooldowns if you really dislike it. Cooldown is **OFF** by default
- Cooldown time for set cooldown, defaults to 2 seconds.

## Changed

- changed 'Yippee' sound to one with less clicky audio

### Enjoy!!

</details>
<hr>

<details>
<summary>1.1.0</summary>

## Added

- Walkie talkie now transmits the sound when shouting 'Yippee!' (minor oversight, my bad!)
- Added config value for the volume of shouting 'Yippee!'.
- Added config value for how far the EnemyAI detection range is when shouting 'Yippee! (if enabled)
- Added config value for how loud the EnemyAI detection volume is when shouting 'Yippee! (if enabled)
- Added syncing to config values using Csync. This means that no matter what your config file is, everything marked with '(HOST ONLY)' will be synced when playing online or in a LAN.

## Changed

- Confetti spam has been reduced, but can be re-enabled in the configs for those who desperately want to cause confetti rain.

</details>
<hr>
<details>
<summary>1.0.0</summary>

## Added

- Yippee Key (Configurable)
- Notify enemies on 'Yippee' (Configurable)
- Confetti (Configurable)
- Debug messages (Configurable)

</details>
