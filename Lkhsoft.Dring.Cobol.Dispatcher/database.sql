CREATE USER ${PSQL_USER} WITH PASSWORD '${PSQL_PASSWORD}';
-- Set default encoding and locale to fr_FR.UTF-8
CREATE DATABASE lkhsoft_dring
    WITH
    OWNER = ${PSQL_USER} -- sets the owner of the database
    ENCODING = 'UTF8' -- sets the character encoding of the database
    LC_COLLATE = 'fr_FR.UTF-8' -- sets the collation rules for sorting strings
    LC_CTYPE = 'fr_FR.UTF-8' -- sets the character classification rules
    TABLESPACE = ${INSTANCE_TABLESPACE_NAME} -- declares the default tablespace
    CONNECTION LIMIT = 256; -- sets a limit on the number of concurrent connections

-- Adds a description to the database
COMMENT ON DATABASE "lkhsoft_dring"
    IS 'Lkhsoft.Dring backoffice project database';

BEGIN;
-- Grant privileges to the database user
GRANT ALL PRIVILEGES ON DATABASE "lkhsoft_dring" TO ${PSQL_USER};
COMMIT;

\c "lkhsoft_dring";

BEGIN;
------------------ TABLES ------------------
    -- Create the 'users' table to store user information
    CREATE TABLE IF NOT EXISTS users
    (
        id        VARCHAR(255) PRIMARY KEY,
        username  VARCHAR(255) NOT NULL UNIQUE,
        password  VARCHAR(255) NOT NULL,
        iv        VARCHAR(255) NOT NULL,
        email     VARCHAR(4096) NOT NULL UNIQUE,
        last_seen DATE          NOT NULL,
        is_online BOOLEAN       NOT NULL
    );
    
    -- Create the 'images' table to store images
    CREATE TABLE IF NOT EXISTS images
    (
        id                  VARCHAR(255) PRIMARY KEY,
        user_id             VARCHAR(255),
        FOREIGN KEY (user_id) REFERENCES users (id) ON DELETE CASCADE,
        title               VARCHAR(255) NOT NULL default '',
        discriminator       VARCHAR(255) NOT NULL default 'ProductImage',
        filename            VARCHAR(255) NOT NULL default '',
        image               BYTEA
    );
    
    -- Create the 'users_profile_picture' table to store user profile pictures
    CREATE TABLE IF NOT EXISTS users_profile_picture
    (
        user_id VARCHAR(255) REFERENCES users (id),
        image_id    VARCHAR(255) REFERENCES images (id),
        PRIMARY KEY (user_id, image_id)
    );

------------------ SEQUENCES ------------------
    
    -- Create the 'users_seq' sequence to generate unique IDs for users
    CREATE SEQUENCE IF NOT EXISTS users_seq START 1;

    -- Create the 'image_seq' sequence to generate unique IDs for images
    CREATE SEQUENCE IF NOT EXISTS images_seq START 1;
    
    -- Create the 'image_seq' sequence to generate unique IDs for images
    CREATE SEQUENCE IF NOT EXISTS users_profile_picture_seq START 1;
    
------------------ ID GENERATION FUNCTIONS ------------------
    
    -- Create the 'user_id_trigger' trigger for automatically generating user IDs
    CREATE OR REPLACE FUNCTION generate_user_id()
        RETURNS TRIGGER AS
    $$
    BEGIN
        NEW.id := CONCAT('user_', NEXTVAL('users_seq'));
        RETURN NEW;
    END;
    $$ LANGUAGE plpgsql;

    -- Create the 'image_id_trigger' trigger for automatically generating image IDs
    CREATE OR REPLACE FUNCTION generate_user_profile_picture_id()
        RETURNS TRIGGER AS
    $$
    BEGIN
        NEW.id := CONCAT('image_', NEXTVAL('users_profile_picture_seq'));
        RETURN NEW;
    END;
    $$ LANGUAGE plpgsql;

    -- Create the 'image_id_trigger' trigger for automatically generating image IDs
    CREATE OR REPLACE FUNCTION generate_image_id()
        RETURNS TRIGGER AS
    $$
    BEGIN
        NEW.id := CONCAT('image_', NEXTVAL('images_seq'));
        RETURN NEW;
    END;
    $$ LANGUAGE plpgsql;

------------------ TRIGGERS ------------------

    -- Create the 'user_id_trigger' trigger for automatically generating user IDs
    CREATE OR REPLACE TRIGGER user_id_trigger
        BEFORE INSERT
        ON users
        FOR EACH ROW
    EXECUTE FUNCTION generate_user_id();

    -- Create the 'image_id_trigger' trigger for automatically generating image IDs
    CREATE OR REPLACE TRIGGER image_id_trigger
        BEFORE INSERT
        ON images
        FOR EACH ROW
    EXECUTE FUNCTION generate_image_id();
    
    -- Create the 'image_id_trigger' trigger for automatically generating image IDs
    CREATE OR REPLACE TRIGGER user_profile_picture_id_trigger
        BEFORE INSERT
        ON users_profile_picture
        FOR EACH ROW
    EXECUTE FUNCTION generate_user_profile_picture_id();

COMMIT;