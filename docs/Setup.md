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
pipenv install
```

## Environment Variables
- OUT_DIR: Path to put before all links and static files, see below for recommended values
  - Production: "/"
  - Local Build: "" (set as empty string)
  - PyCharm Development Server: "/outer-wilds-new-horizons/docs/out/"
- BASE_URL: Base url of the website we're hosting on
  - Local: Leave blank
  - Local (but wanting to test open-graph/twitter): "https://nh.outerwildsmods.com/"
  - Production: "https://nh.outerwildsmods.com/"

## Generating
Run `generate.py` with pipenv
```shell
pipenv run python generate.py
```

## Opening
- Production: Go to the site
- Local: Open `out/index.html`
- PyCharm Development Server: Right click `out/index.html` -> Open In -> Browser -> Default