local drawableSprite = require("structs.drawable_sprite")

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
            cubeName = "cube0",
            colorPalette = "FFFFFF,FFFF00,0000FF,008000,FF0000,FFA500",
            backgroundPalette = "000000,FFFFFF,808080,"
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
        },
        colorPalette = {
            fieldType = "list",
            minimumElements = 6,
            maximumElements = 6,
            elementDefault = "ffffff",
            elementOptions = {
                fieldType = "color",
                allowXNAColors = false
            }
        },
        backgroundPalette = {
            fieldType = "list",
            minimumElements = 3,
            maximumElements = 3,
            elementDefault = "ffffff",
            elementOptions = {
                fieldType = "color",
                allowXNAColors = false
            }
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
        "persistent",
        "colorPalette",
        "backgroundPalette"  
    },
}

local stickerSpacing = 9
local faceGap = 1

local function getFacePositions(size, layout)
    local faceSpacing = stickerSpacing * size + faceGap

    if layout == "NetL" then
        return {
            {2 * faceSpacing, 2 * faceSpacing},
            {2 * faceSpacing, 0},
            {faceSpacing, faceSpacing},
            {3 * faceSpacing, faceSpacing},
            {2 * faceSpacing, faceSpacing},
            {0, faceSpacing},
        }
    else -- NetR
        return {
            {faceSpacing, 2 * faceSpacing},
            {faceSpacing, 0},
            {0, faceSpacing},
            {2 * faceSpacing, faceSpacing},
            {faceSpacing, faceSpacing},
            {3 * faceSpacing, faceSpacing},
        }
    end
end

function rubiksCube.sprite(room, entity)
    local size = entity.size or 3
    local layout = entity.cubeLayout or "NetL"

    local sprites = {}
    local facePositions = getFacePositions(size, layout)

    for _, face in ipairs(facePositions) do
        local fx, fy = face[1], face[2]

        for row = 0, size - 1 do
            for col = 0, size - 1 do
                local sprite = drawableSprite.fromTexture(
                    "objects/WrenbowHelper/RubiksCube/RubiksSticker",
                    entity
                )

                sprite:setPosition(
                    fx + col * stickerSpacing + 5 + entity.x,
                    fy + row * stickerSpacing + 5 + entity.y
                )

                table.insert(sprites, sprite)
            end
        end
    end

    return sprites
end

function rubiksCube.selection(room, entity)
    local size = entity.size or 3
    local faceSpacing = stickerSpacing * size + faceGap

    if (entity.cubeLayout or "NetL") == "NetL" then
        return {
            x = entity.x,
            y = entity.y,
            width = faceSpacing * 4,
            height = faceSpacing * 3
        }
    else
        return {
            x = entity.x,
            y = entity.y,
            width = faceSpacing * 4,
            height = faceSpacing * 3
        }
    end
end

return rubiksCube