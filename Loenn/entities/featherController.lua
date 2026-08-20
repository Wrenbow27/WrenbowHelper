local featherController = {
    name = "WrenbowHelper/FeatherController",

    placements = {
    {
        name = "feather_controller",
        data = {
            flyTime = 2.0,
            highFlySpeed = 190.0,
            turnFlySpeed = 140.0,
            lowFlySpeed = 91.0,
            turnRateMultiplier = 1.0,
            octalAnalog = false,
            featherHyper = false,
            trueFeatherSuper = false,
            trueFeatherWallBounce = false,
            noStartSlowdown = false,
            noFeatherBoost = false,
            trailColor = "ffd65c"
            }
        }
    },
    fieldInformation = {
        trailColor = {
            fieldType = "color",
            allowXNAColors = true,
            useAlpha = false
        },
    },
    texture = "objects/WrenbowHelper/FeatherController/FeatherController"
}

return featherController