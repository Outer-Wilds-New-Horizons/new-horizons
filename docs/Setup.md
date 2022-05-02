# Setup to build docs

## Requirements
- Python 3.10

## Clone the repo
Clone the entire repo and navigate to the docs folder
```shell
git clone https://github.com/xen-42/outer-wilds-new-horizons
cd outer-wilds-new-horizons/docs
```

## Setup Pipenv
Install pipenv if you haven't already
```shell
pip install --user pipenv
```
Install dependencies
```shell
pipenv install --dev
```

## Environment Variables
- URL_PREFIX: Path to put before all links and static files, see below for recommended values
  - Production: "/"
  - Local Build: "" (set as empty string)
  - PyCharm Development Server: "/outer-wilds-new-horizons/docs/out/"


## Copy Schemas
Create a folder called `schemas` in the `docs/content/pages/` folder and copy all schemas to generate into it, make sure not to add this folder to git.
Production build automatically copies over schemas.

## Generating
Run `generate` with pipenv
```shell
pipenv run menagerie generate
```

## Opening
- Production: Go to the site
- Local: Open `out/index.html`
- PyCharm Development Server: Right click `out/index.html` -> Open In -> Browser -> Default
