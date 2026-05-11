local function ignoreAttr(entity)
    local attrs = {"_id", "_name"}

    local advanced = {
        "spritePath",
        "spriteColor",
        "respawnTime",
        "freezeFrames",
        "collectSound",
        "triggerAlways",
        "refillDashes",
        "refillStamina"
    }

    if not entity.moreOptions then
        for _, v in ipairs(advanced) do
            table.insert(attrs, v)
        end
    end

    return attrs
end

local bluetoothRefill = {
    name = "WrenbowHelper/BluetoothRefill",

    placements = {
    {
        name = "bluetooth_refill",
        data = {
            spritePath = "",
            spriteColor = "ffffff",
            respawnTime = 2.5,
            freezeFrames = 3,
            collectSound = "event:/game/general/diamond_touch",
            playerCollect = false,
            triggerViaDashCount = true,
            triggerViaStamina = true,
            triggerAlways = false,
            refillDashes = true,
            refillStamina = true,
            oneUse = false,
            twoDashes = false,
            moreOptions = false
            }
        }
    },
    fieldInformation = {
        spritePath = {
            fieldType = "string"
        },
        spriteColor = {
            fieldType = "color",
            allowXNAColors = true,
            useAlpha = false
        },

        respawnTime = {
            fieldType = "number",
            minimumValue = 0.0167
        },

        freezeFrames = {
            fieldType = "integer",
            minimumValue = 0
        },

        collectSound = {
            fieldType = "string"
        }
    },
    fieldOrder = {
        "x",
        "y",
        "spritePath",
        "spriteColor",
        "respawnTime",
        "freezeFrames",
        "collectSound",
        "playerCollect",
        "triggerViaDashCount",
        "triggerViaStamina",
        "triggerAlways",
        "refillDashes",
        "refillStamina",
        "oneUse",
        "twoDashes",
        "moreOptions"        
    },
    ignoredFields = function(entity)
        return ignoreAttr(entity)
    end,
    texture = function(room, entity)
        if entity.spritePath == "" then
            return not(entity.twoDashes) and "objects/WrenbowHelper/BluetoothRefill/idle00" or "objects/WrenbowHelper/BlueTwothRefill/idle00"
        else
            if string.sub(entity.spritePath, -1) == "/" then
                return entity.spritePath .. "idle00"
            else
                return entity.spritePath .. "/idle00"
            end
        end
    end
}

return bluetoothRefill