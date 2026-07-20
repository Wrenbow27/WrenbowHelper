local displayTypes = {"NetL","NetR"}

local rubiksCube = {
    name = "WrenbowHelper/RubiksCube",

    placements = {
        {
        name = "rubiks_cube",
        data = {
            size = 3,
            startScrambled = true,
            scrambleLength = 30,
            solvedFlag = "WrenbowHelper_RubiksCube0Solved",
            displayType = "NetL",
            lockOnSolve = true,
            persistent = true,
            cubeName = ""
            }
        }
    },
    fieldInformation = {
        size = {
            fieldType = "integer",
            minimumValue = 2
        },
        scrambleLength = {
            fieldType = "integer",
            minimumValue = 0
        },
        solvedFlag = {
            fieldType = "string"
        },
        displayType = {
            options = displayTypes
        },
        cubeName = {
            fieldType = "string"
        }
    },
}

return rubiksCube