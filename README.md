# Mod Validator
Mod Validator is a mod to easily create required, blacklisted, or require an entire list of mods. 


## Creating your config
The base config. Stored at `GameDir/BepInEx/config/ModValidator`
> I'm retarded and don't know how to update a config, if you are updating there will be a new config with the version tag just edit the highest version

**The only config that matters is the server hosts**

    <configuration>
	    <whitelist>true</whitelist>
	    <whitelistedMods> </whitelistedMods>
	    <blacklistedMods> </blacklistedMods>
	    <requiredMods> </requiredMods>
	    <kickIfMissingValidator>true</kickIfMissingValidator>
    </configuration>

`whitelist` is a true/false. True tells the mod to **only** read the whitelisted mods; the clients mod list must be the same as the whitelisted mods if this setting is on. If the whitelist is off, `requiredMods` and `blacklistedMods` are read from.
**Note: When using the whitelist, ModValidator does not need to be in the list. There is a separate check for it.**

> If you would like to create a mod whitelist of your mods, The list of all of your mod names is printed into the debug console on game load. **Make sure the spelling and capitalization is exact**.

`kickifMissingValidator` is a true/false that tells the server to kick a player if they are missing ModValidator.
`whitelistedMods` is a list of comma,seperated,mods **NO SPACES IN THIS LIST**.
`blacklistedMods` is a list of comma,seperated,mods **NO SPACES IN THIS LIST**.
`requiredMods` is a list of comma,seperated,mods **NO SPACES IN THIS LIST**.

## Example Config


    <configuration>
	    <whitelist>true</whitelist>
	    <whitelistedMods>ShareSuite,ChunkyLobbies,ShareEm</whitelistedMods>
	    <!--Not required as whitelist is enabled-->
	    <blacklistedMods>  </blacklistedMods>
	    <requiredMods>  </requiredMods>
	    <!--Kick player if they are missing ModValidator mod-->
	    <kickIfMissingValidator>true</kickIfMissingValidator>
    </configuration>
