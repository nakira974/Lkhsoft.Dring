#!/bin/bash

# Chemin du tablespace
TABLESPACE_DIR="/var/lib/postgresql/tablespaces/${INSTANCE_TABLESPACE_NAME}"

# Créer le répertoire du tablespace et définir les permissions
mkdir -p "${TABLESPACE_DIR}"
chown -R postgres:postgres "${TABLESPACE_DIR}"
chmod 700 "${TABLESPACE_DIR}"

# Création du tablespace dans PostgreSQL
service postgresql start
sudo -E -u postgres psql -c "CREATE TABLESPACE ${INSTANCE_TABLESPACE_NAME} LOCATION '${TABLESPACE_DIR}';"
service postgresql stop

# Afficher un message de confirmation
echo "Tablespace ${INSTANCE_TABLESPACE_NAME} créé dans ${TABLESPACE_DIR}"