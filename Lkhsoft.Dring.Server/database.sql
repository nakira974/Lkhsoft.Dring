-- Create the table Users if it does not exist
CREATE TABLE IF NOT EXISTS Users
(
    Username
        TEXT
        PRIMARY
            KEY,
    Password
        TEXT
        NOT
            NULL,
    IsConnected
        INTEGER
        NOT
            NULL
        DEFAULT
            0,
    IV,
    TEXT
        NOT
            NULL
);

-- Cretae the table Sessions if it does not exist
CREATE TABLE IF NOT EXISTS Sessions
(
    SessionId
        TEXT
        PRIMARY
            KEY,
    Username
        TEXT
        NOT
            NULL,
    StartTime
        TEXT
        NOT
            NULL,
    EndTime
        TEXT,
    FOREIGN
        KEY (Username)
        REFERENCES Users (Username)
);