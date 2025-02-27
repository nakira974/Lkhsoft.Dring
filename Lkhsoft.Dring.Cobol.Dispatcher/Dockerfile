# Utiliser l'image Debian 12.9
FROM debian:12.9

# Définir les variables d'environnement pour le locale et le clavier
ENV LANG fr_FR.UTF-8
ENV LANGUAGE fr_FR:fr
ENV LC_ALL fr_FR.UTF-8

# Installer les locales et configurer fr_FR.UTF-8
RUN apt-get update && \
    apt-get install -y locales && \
    sed -i 's/# fr_FR.UTF-8 UTF-8/fr_FR.UTF-8 UTF-8/' /etc/locale.gen && \
    locale-gen fr_FR.UTF-8 && \
    update-locale LANG=fr_FR.UTF-8

# Configurer le clavier en AZERTY au niveau du système
RUN apt-get install -y keyboard-configuration && \
    echo "XKBMODEL=\"pc105\"\nXKBLAYOUT=\"fr\"\nXKBVARIANT=\"\"\nXKBOPTIONS=\"\"" > /etc/default/keyboard && \
    dpkg-reconfigure -f noninteractive keyboard-configuration

# Nettoyer le cache des paquets pour réduire la taille de l'image
RUN apt-get clean && rm -rf /var/lib/apt/lists/*

# Port du serveur de dispatch
ARG server_port=9333
# Utilisateur de la base de données
ARG DATABASE_USER=transmitter
# Mot de passe de la base de données
ARG DATABASE_PASSWORD=changeit
# Domaine du serveur mail
ARG DOMAIN_NAME=internal.lkh.coffee
# Nom du tablespace pgsql
ARG TABLESPACE_NAME=tablespace_0001

RUN apt-get update \
 && apt-get install -y sudo 

RUN adduser --disabled-password --gecos '' docker
RUN adduser docker sudo
RUN echo '%sudo ALL=(ALL) NOPASSWD:ALL' >> /etc/sudoers

USER docker

# Mettre à jour et installer les dépendances nécessaires
RUN sudo apt-get update && sudo apt-get install -y \
    systemd \
    build-essential \
    wget \
    gpg \
    gnupg2 \
    software-properties-common \
    apt-transport-https \
    lsb-release \
    ca-certificates \
    postfix \
    postgresql \
    libpq-dev \
    postgresql-contrib \
    gnucobol4 \
    cmake \
    make \
    automake \
    libtool \
    autoconf \
    pkg-config \
    bison \
    flex \
    gawk \
    libncurses5 \
    libncurses5-dev \
    libgmp-dev \
    tree
# Créer un utilisateur de service 'transmitter'

RUN sudo groupadd transmitter
RUN sudo useradd -r -s /usr/sbin/nologin -g transmitter transmitter

# Installer PowerShell
RUN sudo wget https://packages.microsoft.com/config/debian/12/packages-microsoft-prod.deb -O packages-microsoft-prod.deb \
    && sudo dpkg -i packages-microsoft-prod.deb \
    && sudo apt-get update \
    && sudo apt-get install -y powershell \
    && sudo rm packages-microsoft-prod.deb

VOLUME /opt/server/ssl
# Configurer Postfix
RUN echo "postfix postfix/main_mailer_type select Internet Site" | sudo debconf-set-selections \
    && echo "postfix postfix/mailname string mail.lkh.coffee" | sudo debconf-set-selections \
    && sudo postconf -e 'myhostname = mail.${DOMAIN_NAME}' \
    && sudo postconf -e 'mydomain = ${DOMAIN_NAME}' \
    && sudo postconf -e 'myorigin = /etc/mailname' \
    && sudo postconf -e 'inet_interfaces = all' \
    && sudo postconf -e 'inet_protocols = ipv4' \
    && sudo postconf -e 'mydestination = mail.${DOMAIN_NAME}, localhost' \
    && sudo postconf -e 'relayhost =' \
    && sudo postconf -e 'smtpd_tls_cert_file=/opt/server/ssl/server_cert.pem' \
    && sudo postconf -e 'smtpd_tls_key_file=/opt/server/ssl/server_cert.key' \
    && sudo postconf -e 'smtpd_use_tls=yes' \
    && sudo postconf -e 'smtpd_tls_session_cache_database = btree:${data_directory}/smtpd_scache' \
    && sudo postconf -e 'smtp_tls_session_cache_database = btree:${data_directory}/smtp_scache' \
    && sudo postconf -e 'smtpd_tls_security_level=encrypt' \
    && sudo postconf -e 'smtpd_tls_auth_only=yes'


# Copier et compiler le projet CMake
COPY ./src /app/transmitter/src
COPY ./headers /app/transmitter/headers
COPY ./CMakeLists.txt /app/transmitter

WORKDIR /app/transmitter
RUN sudo mkdir build && cd build && sudo cmake .. && sudo cmake --build . && sudo cmake --install .

# Copier le fichier de service daemon
COPY transmitter.service /etc/systemd/system/transmitter.service

# Copier et compiler les programmes COBOL
COPY programs /opt/cobol/programs
COPY modules /opt/cobol/modules
COPY modules/accessors /opt/cobol/modules/accessors

# Télécharger OCESQL
ENV CPPFLAGS=-I/usr/include/postgresql
RUN sudo wget https://github.com/opensourcecobol/Open-COBOL-ESQL/archive/refs/tags/v1.4.tar.gz -O /tmp/ocesql.tar.gz
RUN sudo tar -xvf /tmp/ocesql.tar.gz -C /usr/local/src && sudo rm /tmp/ocesql.tar.gz
WORKDIR /usr/local/src/Open-COBOL-ESQL-1.4
RUN sudo sh configure
RUN sudo autoreconf -fvi
RUN sudo -E sh configure
RUN ls -la
RUN sudo make
RUN sudo make install
RUN sudo ldconfig
RUN ls -l /usr/local/lib/libocesql.so

# Variables d'environnement pour que les programmes COBOL trouvent les modules
RUN export COBCPY=/usr/local/src/Open-COBOL-ESQL-1.4/copy && COB_PRE_LOAD=/usr/local/lib/libocesql.so

# Étape 1 : Pré-compiler les accesseurs physiques COBOL (si le dossier n'est pas vide)
RUN if [ -n "$(ls -A /opt/cobol/modules/accessors/*.cbl 2>/dev/null)" ]; then \
        for file in /opt/cobol/modules/accessors/*.cbl; do \
            sudo -E ocesql "$file" "/usr/local/lib/$(basename "$file" .cbl).cob"; \
        done \
    fi

# Étape 2 : Compiler les accesseurs physiques COBOL (si le dossier n'est pas vide)
RUN if [ -n "$(ls -A /opt/cobol/modules/accessors/*.cbl 2>/dev/null)" ]; then \
        for file in /opt/cobol/modules/accessors/*.cbl; do \
            sudo -E cobc -m -locesql "$file" -o "/usr/local/lib/$(basename "$file" .cbl).so"; \
        done \
    fi

# Étape 3 : Compiler les modules COBOL (si le dossier n'est pas vide)
RUN if [ -n "$(ls -A /opt/cobol/modules/*.cbl 2>/dev/null)" ]; then \
        for file in /opt/cobol/modules/*.cbl; do \
            # Vérifier si des accesseurs physiques sont présents
            if [ -n "$(ls -A /opt/cobol/modules/accessors/*.cbl 2>/dev/null)" ]; then \
                # Compiler le module en liant les accesseurs physiques (s'ils existent)
                sudo -E cobc -m -locesql "$file" -o "/usr/local/lib/$(basename "$file" .cbl).so" \
                     -L /usr/local/lib $(ls /usr/local/lib/*.so | sed 's/^/-l /' | tr '\n' ' '); \
            else \
                # Compiler le module sans lier les accesseurs physiques
                sudo -E cobc -m -locesql "$file" -o "/usr/local/lib/$(basename "$file" .cbl).so"; \
            fi \
        done \
    fi

# Étape 4 : Compiler les programmes COBOL (si le dossier n'est pas vide)
RUN if [ -n "$(ls -A /opt/cobol/programs/*.cbl 2>/dev/null)" ]; then \
        for file in /opt/cobol/programs/*.cbl; do \
            # Extraire le nom du programme sans extension
            program_name=$(basename "$file" .cbl); \
            # Vérifier si des modules ou accesseurs physiques sont présents
            if [ -n "$(ls -A /opt/cobol/modules/*.so 2>/dev/null)" ]; then \
                # Compiler le programme en liant les modules et accesseurs physiques (s'ils existent)
                sudo -E cobc -x "$file" -o "/usr/local/bin/$program_name" \
                     -L /usr/local/lib -locesql $(ls /usr/local/lib/*.cbl | sed 's/^/-l /' | tr '\n' ' '); \
            else \
                # Compiler le programme sans lier les modules ni les accesseurs physiques
                sudo -E cobc -x -locesql "$file" -o "/usr/local/bin/$program_name"; \
            fi \
        done \
    fi



# Installer gettext pour utiliser envsubst
RUN sudo apt-get update && sudo apt-get install -y gettext && sudo rm -rf /var/lib/apt/lists/*

# Copier le script SQL dans un répertoire temporaire
COPY database.sql /tmp/database.sql

# Remplacer les placeholders dans le script SQL
ENV PSQL_USER=${DATABASE_USER}
ENV PSQL_PASSWORD=${DATABASE_PASSWORD}

# Création du tablespace
ENV INSTANCE_TABLESPACE_NAME=${TABLESPACE_NAME}
COPY create_tablespace.sh /tmp/create_tablespace.sh
RUN sudo chmod +x /tmp/create_tablespace.sh
RUN sudo -E sh /tmp/create_tablespace.sh

# Supprimer le script de création du tablespace
RUN sudo rm /tmp/create_tablespace.sh

# Remplacer les placeholders dans le script SQL
RUN envsubst < /tmp/database.sql > /tmp/create_database.sql
RUN sudo cat /tmp/create_database.sql

# Création de la base de données et suppression du script SQL avec le mdp en clair
RUN  sudo service postgresql start &&  \
     sudo -E -u postgres psql -f /tmp/create_database.sql && \
     sudo service postgresql stop && \
     sudo rm /tmp/create_database.sql && sudo rm /tmp/database.sql

VOLUME /var/lib/postgresql/data
VOLUME /var/lib/postgresql/tablespaces

# Exposer les ports nécessaires
EXPOSE 25 465 5432 $server_port

RUN sudo chmod 644 /etc/systemd/system/transmitter.service

# Démarrer les services nécessaires
RUN sudo systemctl enable postfix && \
    sudo systemctl enable postgresql
RUN sudo systemctl enable transmitter

USER root
# Démarrer systemd comme processus principal
CMD ["/sbin/init"]