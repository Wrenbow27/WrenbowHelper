local flagIfLineOfSight = {
    name = "WrenbowHelper/FlagIfLineOfSight",
    placements = {
        {
            name = "flag_if_line_of_sight",
            data = {
                entity1 = "self",
                entity2 = "player",
                flag = "",
                flagIf = "",
                maxRange = 60,
                solids = true,
                lightBlockers = false,
            }
        },
    },
    fieldInformation = {
    },
}

return flagIfLineOfSight