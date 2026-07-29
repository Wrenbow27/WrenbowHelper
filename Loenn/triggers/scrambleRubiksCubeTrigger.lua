local scrambleRubiksCubeTrigger = {
    name = "WrenbowHelper/ScrambleRubiksCubeTrigger",
    placements = {
        {
            name = "scramble_rubiks_cube_trigger",
            data = {
                cubeName = "cube0",
                scrambleLength = 30,
                doOnce = true
            }
        },
    },
    fieldInformation = {
        scrambleLength = {
            fieldType = "integer",
            minimumValue = 0
        }
    },
}

return scrambleRubiksCubeTrigger