## Features

- Allows storage monitors to be deployed to more container types
- Configurable position and rotation for each container type
- Self-service support for adding more container types in the config

Tip: Storage monitors don't update realtime in the Rust+ companion app when only the stack amount of an item in the container changes, but you can minimize (or close) the app and then reopen it to see up-to-date information.

## Permissions

Note: Permissions are only necessary if the corresponding container type's configuration has specified `"Require permission": true`. The default is `false` for all preloaded container types.

- `storagemonitorcontrol.owner.all` -- All containers deployed by players with this permission will be eligible to receive a storage monitor, as long as those container types are enabled in the plugin configuration.

As an alternative to the `all` permission, you can grant permissions by container type. These are automatically generated from the plugin configuration.

- `storagemonitorcontrol.owner.bbq`
- `storagemonitorcontrol.owner.coffinstorage`
- `storagemonitorcontrol.owner.composter`
- `storagemonitorcontrol.owner.crudeoutput`
- `storagemonitorcontrol.owner.dropbox`
- `storagemonitorcontrol.owner.fridge`
- `storagemonitorcontrol.owner.fuelstorage`
- `storagemonitorcontrol.owner.furnace`
- `storagemonitorcontrol.owner.furnace.large`
- `storagemonitorcontrol.owner.guntrap`
- `storagemonitorcontrol.owner.hitchtrough`
- `storagemonitorcontrol.owner.hopperoutput`
- `storagemonitorcontrol.owner.locker`
- `storagemonitorcontrol.owner.mailbox`
- `storagemonitorcontrol.owner.mixingtable`
- `storagemonitorcontrol.owner.planter.small`
- `storagemonitorcontrol.owner.planter.large`
- `storagemonitorcontrol.owner.refinery_small`
- `storagemonitorcontrol.owner.survivalfishtrap`
- `storagemonitorcontrol.owner.woodbox`

Note: When requiring permissions, the container must have been deployed while the player had permission or that container won't be eligible to receive a storage monitor until the next server restart. This can be improved upon request.

## Configuration

Default configuration (no additional containers can receive storage monitors):

```json
{
  "Containers": {
    "bbq.deployed": {
      "Enabled": false,
      "Require permission": false,
      "Position": {
        "x": 0.1,
        "y": 0.0,
        "z": 0.3
      },
      "Rotation angles": {
        "x": 0.0,
        "y": 90.0,
        "z": 0.0
      }
    },
    "coffinstorage": {
      "Enabled": false,
      "Require permission": false,
      "Position": {
        "x": -1.15,
        "y": 0.196,
        "z": 0.0
      },
      "Rotation angles": {
        "x": 90.0,
        "y": 0.0,
        "z": 90.0
      }
    },
    "composter": {
      "Enabled": false,
      "Require permission": false,
      "Position": {
        "x": 0.0,
        "y": 1.54,
        "z": 0.4
      }
    },
    "crudeoutput": {
      "Enabled": false,
      "Require permission": false,
      "Position": {
        "x": -0.4,
        "y": 0.0,
        "z": 2.5
      },
      "Rotation angles": {
        "x": 0.0,
        "y": 90.0,
        "z": 0.0
      }
    },
    "dropbox.deployed": {
      "Enabled": false,
      "Require permission": false,
      "Position": {
        "x": 0.3,
        "y": 0.545,
        "z": -0.155
      },
      "Rotation angles": {
        "x": 0.0,
        "y": 184.0,
        "z": 0.0
      }
    },
    "fridge.deployed": {
      "Enabled": false,
      "Require permission": false,
      "Position": {
        "x": -0.2,
        "y": 1.995,
        "z": 0.2
      },
      "Rotation angles": {
        "x": 0.0,
        "y": 10.0,
        "z": 0.0
      }
    },
    "fuelstorage": {
      "Enabled": false,
      "Require permission": false,
      "Position": {
        "x": -1.585,
        "y": -0.034,
        "z": 0.0
      }
    },
    "furnace": {
      "Enabled": false,
      "Require permission": false,
      "Position": {
        "x": 0.0,
        "y": 1.53,
        "z": 0.05
      }
    },
    "furnace.large": {
      "Enabled": false,
      "Require permission": false,
      "Position": {
        "x": 0.31,
        "y": 0.748,
        "z": -1.9
      },
      "Rotation angles": {
        "x": 0.0,
        "y": 190.0,
        "z": 0.0
      }
    },
    "guntrap.deployed": {
      "Enabled": false,
      "Require permission": false,
      "Position": {
        "x": 0.0,
        "y": 0.032,
        "z": -0.3
      },
      "Rotation angles": {
        "x": 0.0,
        "y": 180.0,
        "z": 0.0
      }
    },
    "hitchtrough.deployed": {
      "Enabled": false,
      "Require permission": false,
      "Position": {
        "x": -0.82,
        "y": 0.65,
        "z": 0.215
      }
    },
    "hopperoutput": {
      "Enabled": false,
      "Require permission": false,
      "Position": {
        "x": -0.71,
        "y": -0.02,
        "z": 1.25
      }
    },
    "locker.deployed": {
      "Enabled": false,
      "Require permission": false,
      "Position": {
        "x": -0.67,
        "y": 2.238,
        "z": 0.04
      },
      "Rotation angles": {
        "x": 0.0,
        "y": 10.0,
        "z": 0.0
      }
    },
    "mailbox.deployed": {
      "Enabled": false,
      "Require permission": false,
      "Position": {
        "x": 0.0,
        "y": 1.327,
        "z": 0.21
      }
    },
    "mixingtable.deployed": {
      "Enabled": false,
      "Require permission": false,
      "Position": {
        "x": -0.9,
        "y": 0.0,
        "z": 0.0
      }
    },
    "planter.small.deployed": {
      "Enabled": false,
      "Require permission": false,
      "Position": {
        "x": -1.22,
        "y": 0.482,
        "z": 0.3
      }
    },
    "planter.large.deployed": {
      "Enabled": false,
      "Require permission": false,
      "Position": {
        "x": -1.22,
        "y": 0.482,
        "z": 1.22
      }
    },
    "refinery_small_deployed": {
      "Enabled": false,
      "Require permission": false,
      "Position": {
        "x": 0.0,
        "y": 2.477,
        "z": 0.0
      },
      "Rotation angles": {
        "x": 0.0,
        "y": 180.0,
        "z": 0.0
      }
    },
    "survivalfishtrap.deployed": {
      "Enabled": false,
      "Require permission": false,
      "Position": {
        "x": 0.0,
        "y": 0.4,
        "z": -0.6
      }
    },
    "woodbox_deployed": {
      "Enabled": false,
      "Require permission": false,
      "Position": {
        "x": -0.24,
        "y": 0.55,
        "z": 0.14
      },
      "Rotation angles": {
        "x": 0.0,
        "y": 10.0,
        "z": 0.0
      }
    }
  }
}
```

Note: The `fuelstorage` prefab refers to the fuel container for mining quarries and pump jacks.

Each container type has the following configuration options, mapped in the config to the container's prefab short name.

- `Enabled` (`true` or `false`) -- Must be `true` for players to be able to deploy storage monitors to containers of this type.
- `Require permission` (`true` or `false`) -- While `true`, only containers of this type that were deployed by players with the corresponding permission will be eligible to receive storage monitors.
  - Note: The container must have been deployed while the player had permission or that container won't be eligible to receive a storage monitor until the next server restart. This can be improved upon request.
- `Position` (`x`, `y`, `z`) -- Position of the storage monitor, relative to the parent container.
- `Rotation angles` -- Rotation of the storage monitor, relative to the parent container.

Many container types are provided in the plugin's default configuration, but you can also add other container types if you know the container's short prefab name. If you add more, please post in the plugin's help forum to suggest it be added to the default so that others can benefit from it.
