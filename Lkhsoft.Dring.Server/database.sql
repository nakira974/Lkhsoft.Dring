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
    Role
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

-- Trigger that updates the IsConnected field of the Users table when a session is created
CREATE TRIGGER IF NOT EXISTS UpdateUserOnSessionEnd
    AFTER UPDATE OF EndTime
    ON Sessions
    FOR EACH ROW
    WHEN NEW.EndTime IS NOT NULL
BEGIN
    UPDATE Users
    SET IsConnected = 0
    WHERE Username = NEW.Username;
END;

-- Trigger that updates the IsConnected field of the Users table when a session is created
CREATE TRIGGER IF NOT EXISTS UpdateUserOnSessionStart
    AFTER INSERT
    ON Sessions
    FOR EACH ROW
BEGIN
    UPDATE Users
    SET IsConnected = 1
    WHERE Username = NEW.Username;
END;