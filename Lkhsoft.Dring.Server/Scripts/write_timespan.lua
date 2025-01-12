-- Script Lua pour écrire dans un fichier toutes les 10 secondes, 6 fois

local filePath = arg[1] or "C:/lua/write_timepsan.output.txt"
local interval = tonumber(arg[2]) or 10  -- Intervalle entre les écritures, ou valeur par défaut (10 secondes)
local iterations = tonumber(arg[3]) or 6  -- Nombre d'itérations, ou valeur par défaut (6 fois)

-- Fonction pour écrire le timespan dans le fichier
local function writeTimespanToFile()
    local file = io.open(filePath, "a")  -- Ouvrir en mode "append"
    if file then
        -- Obtenir l'heure actuelle sous forme de string
        local currentTime = os.date("%Y-%m-%d %H:%M:%S")
        file:write("Timespan: " .. currentTime .. "\n")
        file:close()
        print("Wrote timespan: " .. currentTime)
    else
        print("Error opening file!")
    end
end

-- Fonction principale pour exécuter l'écriture toutes les 10 secondes, 6 fois
local function scheduleBatchExecution()
    -- Répéter les écritures
    for i = 1, iterations do
        -- Attendre l'intervalle spécifié (en secondes)
        sleep(interval)
        -- Écrire le timespan dans le fichier
        writeTimespanToFile()
    end
end
-- Fonction pour réaliser une pause de n secondes
function sleep(sec)
    local start = os.time()
    while os.time() - start < sec do end
end

-- Démarrer l'exécution du script
scheduleBatchExecution()