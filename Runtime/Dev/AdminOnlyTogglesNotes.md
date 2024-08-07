
# How To Go About Toggling

## The Problem

Toggles are implemented using synced scripts. Having a toggle that disables toggles for non admins prevents them from syncing.

## The Probably Most Obvious Solution

This is a non functional solution as it runs into the problem described above. It's basically a local toggle game objects script which toggles the toggles which are meant for admins only.

## A Dumb But Technically Possible Solution

Have an interact proxy, which is the object which gets toggled by the admin only toggle, and then those interact proxies point to scripts which remain always active.

## The Most Basic Solution

A script to toggle the active state of colliders. The upside is that its super simple, the downside is that it's cumbersome to use and prone to user (map creator) error.

## Slightly More Sophisticated Solution

Functionally the same as the previous one except that the list of colliders isn't provided through the inspector but rather they are found using get components in children. This requires very little setup, however it has the issue that there are now game objects which are associated/belong to a given area of the map however they are not in the hierarchy of that part of the map. So it is an organizational issue, and prevents easily moving things around in the scene once they're setup.

## A Flexible Solution

Still functionally the same - toggling colliders - however this time the colliders to toggle get found at build time using an editor script which searches for a specific custom editor only component and then gets the colliders on the same object as that component. It then saves that list on a serialized but hidden field on the admin only toggle script.

# How To Manage Admins

You can make this really simple or complicated. But having some way to change admins at runtime is **highly** recommended, as I know from experience that there's going to be times where people who are supposed to be admins aren't, or you need temporary ones. I'd also recommend making it case insensitive.
