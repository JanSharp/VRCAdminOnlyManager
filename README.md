
# Admin Only Objects

Mark objects as only usable by admins and manage said admins.

# Installing

Head to my [VCC Listing](https://jansharp.github.io/vrc/vcclisting.xhtml) and follow the instructions there.

# Features

- AdminOnlyManager
  - List of default admins
  - Option for admin names to be compared with or without case sensitivity
  - Optional UI Toggle
    - to show if the local player is admin
    - to toggle the local player being admin
  - Optional Interact
    - to toggle the local player being admin
- AdminOnlyMarker
  - Used by an editor script in order for AdminOnlyManager to know which objects are for admins only
  - If the object a marker is on has one of the following components, that component will be managed
    - Any type of collider - disabled for non admins
    - Any type of renderer - disabled for non admins
    - Canvas - disabled for non admins
    - CanvasGroup - for non admins: Alpha set to 0, Interactable set to false, Blocks Raycasts set to false
