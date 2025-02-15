# Port du serveur de dispatch
ARG server_port=9333
# Chemin du certificat SSL
ARG CERTIFICATE_PATH
# Utilisateur de la base de données
ARG DATABASE_USER=transmitter
# Mot de passe de la base de données
ARG DATABASE_PASSWORD=changeit


# Utiliser l'image Debian 12.9
FROM debian:12.9

RUN mkdir /opt/server/ssl

COPY ${CERT_PATH}/cert.pem /opt/server/ssl/server_cert.pem
COPY ${CERT_PATH}/key.key /opt/server/ssl/server_cert.key

# Mettre à jour et installer les dépendances nécessaires
RUN apt-get update && apt-get install -y \
    systemd \
    build-essential \
    wget \
    gpg \
    gnupg2 \
    software-properties-common \
    apt-transport-https \
    lsb-release \
    ca-certificates \
    curl \
    ufw \
    postfix \
    postgresql \
    open-cobol \
    cmake \
    && rm -rf /var/lib/apt/lists/*

# Créer un utilisateur de service 'transmitter'
RUN useradd --system --no-create-home --shell /usr/sbin/nologin transmitter

# Installer PowerShell
RUN wget https://packages.microsoft.com/config/debian/12/packages-microsoft-prod.deb -O packages-microsoft-prod.deb \
    && dpkg -i packages-microsoft-prod.deb \
    && apt-get update \
    && apt-get install -y powershell \
    && rm packages-microsoft-prod.deb

# Configurer Postfix
RUN echo "postfix postfix/main_mailer_type select Internet Site" | debconf-set-selections \
    && echo "postfix postfix/mailname string mail.lkh.coffee" | debconf-set-selections \
    && postconf -e 'myhostname = mail.lkh.coffee' \
    && postconf -e 'mydomain = lkh.coffee' \
    && postconf -e 'myorigin = /etc/mailname' \
    && postconf -e 'inet_interfaces = all' \
    && postconf -e 'inet_protocols = ipv4' \
    && postconf -e 'mydestination = mail.lkh.coffee, localhost' \
    && postconf -e 'relayhost =' \
    && postconf -e 'smtpd_tls_cert_file=/opt/server/ssl/server_cert.pem' \
    && postconf -e 'smtpd_tls_key_file=/opt/server/ssl/server_cert.key' \
    && postconf -e 'smtpd_use_tls=yes' \
    && postconf -e 'smtpd_tls_session_cache_database = btree:${data_directory}/smtpd_scache' \
    && postconf -e 'smtp_tls_session_cache_database = btree:${data_directory}/smtp_scache' \
    && postconf -e 'smtpd_tls_security_level=encrypt' \
    && postconf -e 'smtpd_tls_auth_only=yes'

# Configurer UFW pour autoriser uniquement l'envoi de mails en SSL
RUN ufw allow 465/tcp \
    && ufw allow 25/tcp \
    && ufw enable $server_port/tcp \
    && ufw allow 5432/tcp 

# Copier et compiler le projet CMake
COPY . /app
WORKDIR /app
RUN cmake --build . && cmake --install .

# Seul le compte de service du serveur de dispatch peut run le service
RUN chown transmitter:transmitter /usr/local/bin/transmitter && \
    chmod 750 /usr/local/bin/transmitter 

# Copier le fichier de service daemon
COPY transmitter.service /etc/systemd/system/transmitter.service

# Copier et compiler les programmes COBOL
COPY programs /opt/cobol/programs
COPY modules /opt/cobol/modules

# Compiler chaque module COBOL et l'installer dans /usr/local/lib
RUN for file in /opt/cobol/modules/*.cob; do \
        cobc -m "$file" -o "/usr/local/lib/$(basename "$file" .cob).so"; \
    done

# Compiler chaque programme COBOL en liant les modules via le repertoire de la librairie locale du compte de service
RUN for file in /opt/cobol/programs/*.cob; do \
        cobc -x "$file" -o "/usr/local/bin/$(basename "$file" .cob)" -L /usr/local/lib -l "$(basename "$file" .cob)"; \
    done

# Variables d'environnement pour que les programmes COBOL trouvent les modules
ENV COB_LIBRARY_PATH=/usr/local/lib
ENV LD_LIBRARY_PATH=/usr/local/lib

# Définir les variables d'environnement
ENV DATABASE_USER=${DATABASE_USER}
ENV DATABASE_PASSWORD=${DATABASE_PASSWORD}

# Configurer PostgreSQL
RUN service postgresql start && \
    su - postgres -c "psql -c \"CREATE USER ${DATABASE_USER} WITH PASSWORD '${DATABASE_PASSWORD}';\"" && \
    service postgresql stop

# Installer gettext pour utiliser envsubst
RUN apt-get update && apt-get install -y gettext && rm -rf /var/lib/apt/lists/*

# Copier le script SQL dans un répertoire temporaire
COPY database.sql /tmp/database.sql

# Remplacer les placeholders dans le script SQL
RUN envsubst '${DATABASE_USER}' < /tmp/database.sql > /docker-entrypoint-initdb.d/database.sql
    
# Exposer les ports nécessaires
EXPOSE 25 465 5432 $server_port

# Démarrer les services nécessaires
RUN systemctl enable postfix && \
    systemctl enable postgresql \
    systemctl enable ufw \
    systemctl enable transmitter \

# Démarrer systemd comme processus principal
CMD ["/lib/systemd/systemd"]