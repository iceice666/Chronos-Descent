
set windows-shell := ["powershell.exe", "-NoLogo", "-Command"]

# List all the just commands
[private]
default:
    @just --list --unsorted

build-image:
    docker build -t chronos-descent-builder Builder

android-debug:
    docker run -v .:/project chronos-descent-builder Android

android-debug-install: android-debug
    adb install build/Chronos-Descent_Android_debug.apk

android-release:
    docker run -v .:/project chronos-descent-builder Android release


windows-debug:
    docker run -v .:/project chronos-descent-builder Windows

windows-release:
    docker run -v .:/project chronos-descent-builder Windows release

linux-debug:
    docker run -v .:/project chronos-descent-builder Linux

linux-release:
    docker run -v .:/project chronos-descent-builder Linux release