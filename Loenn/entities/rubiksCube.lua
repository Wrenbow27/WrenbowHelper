local cubeLayouts = {"NetL","NetR"}

local rubiksCube = {
    name = "WrenbowHelper/RubiksCube",

    placements = {
        {
        name = "rubiks_cube",
        data = {
            size = 3,
            startScrambled = true,
            scrambleLength = 30,
            solvedFlag = "WrenbowHelper_RubiksSolved_cube0",
            cubeLayout = "NetL",
            lockOnSolve = true,
            persistent = true,
            cubeName = "cube0"
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
        cubeLayout = {
            options = cubeLayouts
        },
        cubeName = {
            fieldType = "string"
        }
    },
    fieldOrder = {
        "x",
        "y",
        "size",
        "cubeLayout",
        "scrambleLength",
        "startScrambled",
        "solvedFlag",
        "lockOnSolve",
        "cubeName",
        "persistent"      
    },
}

return rubiksCube