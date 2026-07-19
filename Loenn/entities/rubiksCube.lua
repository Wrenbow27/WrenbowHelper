local displayTypes = {"NetL"}

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
            CubeID = "Cube0"
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
        CubeID = {
            fieldType = "string"
        }
    },
}

return rubiksCube