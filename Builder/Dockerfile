#
# Reference:
# https://github.com/abarichello/godot-ci/blob/master/mono.Dockerfile
#

FROM --platform=linux/amd64 ubuntu:22.04

USER root
SHELL ["/bin/bash", "-c"]
ENV DEBIAN_FRONTEND=noninteractive

# Set environment variables for Chronos Descent
ENV GODOT_VERSION=4.4 \
    RELEASE_NAME=stable \
    EXPORT_NAME=Chronos-Descent \
    PROJECT_PATH=/project \
    ANDROID_HOME=/usr/lib/android-sdk

# Install dependencies
RUN apt-get update && apt-get install -y --no-install-recommends \
    gnupg \
    ca-certificates \
    && apt-key adv --keyserver hkp://keyserver.ubuntu.com:80 --recv-keys 3FA7E0328081BFF6A14DA29AA6A19B38D3D831EF \
    && echo "deb https://download.mono-project.com/repo/ubuntu stable-focal main" | tee /etc/apt/sources.list.d/mono-official-stable.list \
    && apt-get update \
    && apt-get install -y --no-install-recommends \
    mono-complete \
    ca-certificates \
    git \
    git-lfs \
    python3 \
    python3-openssl \
    unzip \
    wget \
    zip \
    openjdk-17-jdk-headless \
    adb \
    rsync \
    wine64 \
    osslsigncode \
    apt-transport-https \
    && rm -rf /var/lib/apt/lists/*

# Set python symlink
RUN ln -s /usr/bin/python3 /usr/bin/python

# Install .NET 8 SDK
RUN wget -q https://dot.net/v1/dotnet-install.sh -O dotnet-install.sh \
    && chmod +x ./dotnet-install.sh \
    && ./dotnet-install.sh --version 8.0.300 --install-dir /usr/share/dotnet \
    && rm dotnet-install.sh
ENV PATH="/usr/share/dotnet:${PATH}"

ADD getbutler.sh /opt/butler/getbutler.sh
RUN bash /opt/butler/getbutler.sh
ENV PATH="/opt/butler/bin:${PATH}"

# Download and set up Android SDK to export to Android.
RUN wget https://dl.google.com/android/repository/commandlinetools-linux-9477386_latest.zip \
    && unzip commandlinetools-linux-*_latest.zip -d cmdline-tools \
    && mkdir -p ${ANDROID_HOME}/cmdline-tools \
    && mv cmdline-tools/cmdline-tools  ${ANDROID_HOME}/cmdline-tools/latest \
    && rm -f commandlinetools-linux-*_latest.zip

ENV PATH="${ANDROID_HOME}/cmdline-tools/latest/bin:${ANDROID_HOME}/platform-tools:${ANDROID_HOME}/build-tools/34.0.0:${PATH}"

# Accept licenses and install Android components
RUN mkdir -p $ANDROID_HOME/licenses \
    && ls -la ${ANDROID_HOME}/cmdline-tools/latest \
    && echo "24333f8a63b6825ea9c5514f83c2829b004d1fee" > $ANDROID_HOME/licenses/android-sdk-license \
    && ${ANDROID_HOME}/cmdline-tools/latest/bin/sdkmanager --update \
    && ${ANDROID_HOME}/cmdline-tools/latest/bin/sdkmanager \
    "platform-tools" "platforms;android-34" "build-tools;34.0.0" "ndk;28.0.13004108" "cmake;3.31.5"

# Add Android keystore and settings.
RUN keytool -keyalg RSA -genkeypair -alias androiddebugkey -keypass android -keystore debug.keystore -storepass android -dname "CN=Android Debug,O=Android,C=US" -validity 9999 \
    && mv debug.keystore /root/debug.keystore

# Download and set up rcedit to change Windows executable icons on export.
RUN wget https://github.com/electron/rcedit/releases/download/v2.0.0/rcedit-x64.exe -O /opt/rcedit.exe

# Download and setup Godot
ADD setup_godot.sh /setup_godot.sh
RUN bash /setup_godot.sh

RUN godot -v -e --quit --headless ${GODOT_TEST_ARGS}

# Godot editor settings
ADD editor_settings.tres /editor_settings.tres
RUN mkdir -p ~/.config/godot/ \
    && ls /editor_settings.tres \
    && mv /editor_settings.tres ~/.config/godot/editor_settings-${GODOT_VERSION:0:3}.tres

# Load the build script
COPY godot_export.sh /build.sh
RUN chmod +x /build.sh

WORKDIR /project
ENTRYPOINT ["/build.sh"]
