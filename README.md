
# Admin Only Objects

Mark objects as only usable by admins and manage said admins.

# Installing

Head to my [VCC Listing](https://jansharp.github.io/vrc/vcclisting.xhtml) and follow the instructions there.

# Features

- AdminOnlyManager
  - List of default admins
  - Option for the admin list to be loaded from a url
    - The admin list loaded from the given url must be formatted as described in [Loaded Admin List Format](#loaded-admin-list-format)
  - Option for admin names to be compared with or without case sensitivity
  - Optional Is Admin UI Toggle
    - to show if the local player is admin
    - to toggle the local player being admin
      - Any changes to the admin list are then ignored, until `RemoveAllOverrides` is raised
  - Optional Interact
    - to toggle the local player being admin
      - Any changes to the admin list are then ignored, until `RemoveAllOverrides` is raised
  - Optional Admin List Input Field
    - to modify the admin list from within the instance, synced
- AdminOnlyMarker
  - Used by an editor script in order for AdminOnlyManager to know which objects are for admins only
  - If the object a marker is on has one of the following components, that component will be managed
    - Any type of collider - disabled for non admins
    - Any type of renderer - disabled for non admins
    - Canvas - disabled for non admins
    - CanvasGroup - for non admins: Alpha set to 0, Interactable set to false, Blocks Raycasts set to false
- Raisable Custom Events, raisable from other scripts or through UI events:
  - `ResetAdminListToDefault` to reset the list to what was set in the inspector (for everyone of course). If something was loaded from the url it would be lost
  - `RemoveAllOverrides` to reset every player's IsAdmin state to match the current admin list, removing any overrides which were applied through the usage of the IsAdmin Toggle, through the Interact event or through third party scripts calling `BecomeAdmin`, `BecomeNonAdmin` or `ToggleIsAdmin`
  - `BecomeAdmin` makes the local player admin regardless of what the admin list says and marks this state as overridden
  - `BecomeAdmin` makes the local player non admin regardless of what the admin list says and marks this state as overridden
  - `ToggleIsAdmin` inverts the local player's admin state and marks this state as overridden
  - `LoadAdminListFromUrl` refreshes the admin list loaded from the url and syncs the new list

# Loaded Admin List Format

- One player name per line
- Leading and trailing spaces are ignored on each line
- Empty lines are ignored
- Any form of newline as line breaks is supported (even just carriage return (nothing uses just carriage returns))
- Player names which contain newlines themselves are not supported

# Using Github Gists For Admin Lists

- Go to https://gist.github.com
- Log in or create a github account
- Press the + in the top right
- Enter whatever you want into the "Gist description"
- Use whatever you want as the filename, I went with "admin-list.txt"
- Put the admin names into the file content, one per line (see [Loaded Admin List Format](#loaded-admin-list-format))
- Create secret gist
- Press the `raw` button near the top right
- Remove the second sha256 from the url
  - Going from https://gist.githubusercontent.com/JanSharp/b9d9fc71311472f4ba9b9c56c0cb6bcc/raw/9cd39a8628530cdb917c633bf55c4e58fc1ed71d/admin-list.txt
  - To https://gist.githubusercontent.com/JanSharp/b9d9fc71311472f4ba9b9c56c0cb6bcc/raw/admin-list.txt
  - This makes the url always point to the latest version of the admin list file
- Paste this url into the Admin List Url field for the AdminOnlyManager
