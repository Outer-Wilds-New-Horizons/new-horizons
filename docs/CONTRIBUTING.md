# Contributing To NH Docs

## Introduction

Thank you for your interest in contributing to NH Docs! We are excited to have you here. Before you start contributing, we would like you to take a few minutes to read through this document. It will help you understand how we work and how you can contribute.

## How Pages Are Organized

Pages in the NH docs are all markdown files. The folder with all the pages is in `src/content/docs`.

- `index.mdx` Is a special file that is the home page of the docs. This is a markdown X file that allows us to use Astro components in the markdown.
- `start-here` is the folder that contains all the pages for the start here section of the docs.
- `getting-started` is the folder that contains all the pages for the getting started section of the docs.
- `reference` is the folder that contains all the pages for the api section of the docs.

Finally, the `schemas` folder contains all the schema pages. You might notice that the schema folder is not present in GitHub. This is because the schema pages are auto-generated from the schema files in `../NewHorizons/Schemas`. In order to edit these you need to edit the C# class they correspond to. More info in the main contributing document found one folder up.

## How To Edit Pages

As said before, all pages are markdown files. This means that you can edit them with any text editor. You can even edit them directly on GitHub. However, we recommend using VS Code with the markdown preview extension. This will allow you to see a preview of the page as you edit it.

One thing to note is the section fenced with `---` at the top of each page. This is the frontmatter. It contains metadata about the page. You will most likely only need to edit the `title` and `description` fields. The `title` field is the title of the page. The `description` field is the description of the page. This is used for SEO purposes. You can view all frontmatter options [on the starlight docs](https://starlight.astro.build/reference/frontmatter/).

If you open this folder (`docs` not the entire repo), VSCode should prompt you to install the recommended extensions. If it doesn't, you can install them manually. The recommended extensions are:

- astro-build.astro-vscode
- davidanson.vscode-markdownlint
- yzhang.markdown-all-in-one
- esbenp.prettier-vscode

## How To Add Pages

Adding pages is very simple. All you need to do is create a new markdown file in the folder you want the page to be in. Then, add the frontmatter to the top of the file. Finally, add the content of the page. You can use the other pages as a reference for how to do this. Advanced pages can be created using Astro components. You can learn more about Astro components [on the Astro docs](https://docs.astro.build/en/core-concepts/astro-components/). You'll need to use `.mdx` instead of `.md` if you want to use Astro components.

## How To Add Images

Images are stored in `src/assets/docs-images`. Each page has a folder in here for its images. To add an image create a folder and name it the name of your page. Then do the following:

```md
![My Image](@/assets/docs-images/<page-name>/<image-name>)
```

Replace `<page-name>` with the name of your page and `<image-name>` with the name of your image.

Your images will be automatically optimized when the site is built.

## Building The Site

If you want to get a local copy of the site running, you'll need a few programs

- [Node.js](https://nodejs.org/en/)
- [PNPM](https://pnpm.io/)

Once you have these installed, you can run the following commands to get the site running locally:

```bash
pnpm i
pnpm dev
```

This will install all the dependencies and start the dev server. You can view the site at `https://localhost:4321/`.

You can also run `pnpm build` to build the site. The built site will be in the `dist` folder.

## Submitting Your Changes

Before anything, please run `pnpm format` on your changes. This will format all the files in the repo. This is important because it ensures that all the files are formatted the same way. This makes it easier to review your changes.

Next, create a new branch for your changes. Then, commit your changes to that branch. Finally, push your branch to GitHub and open a pull request with the `documentation` label.
