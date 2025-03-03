# List all the just commands
[private]
default:
    @just --choose

build-image:
    docker build -t chronos-descent-builder Builder

android-debug:
    docker run -v .:/project chronos-descent-builder

android-release:
    docker run -v .:/project chronos-descent-builder release
