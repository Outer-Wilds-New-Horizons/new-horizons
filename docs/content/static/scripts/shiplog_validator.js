// Clay made all of this


// filesContents is an array of the contents of the xml files to validate with
// throws and exception if the files are not valid xml
const parse = (filesContents) => {
    const parser = new DOMParser()
    const xmlDocs = filesContents.map((contents, index) => {
            try {
                return parser.parseFromString(contents, "text/xml")
            } catch {
                throw new Error(`File ${index} was unable to be parsed.`)
            }
        }
    )
    return xmlDocs
}

// throws and exception if the files are invalid
// otherwise does not
const validate = (xmlDocs) => {
    const planetIDs = xmlDocs.map((doc, index) => getChildByTag(doc.documentElement, "ID", `Document ${index} does not have a planet id.`).innerHTML)
    const allEntryIDs = getAllIDsForTagType(xmlDocs, "Entry")

    const allRumorFactIDs = getAllIDsForTagType(xmlDocs, "RumorFact")
    const allExploreFactIDs = getAllIDsForTagType(xmlDocs, "ExploreFact")
    const allFactIDs = allExploreFactIDs.concat(allRumorFactIDs)

    // validate id uniqueness
    checkForDuplicateIDs(allEntryIDs)
    checkForDuplicateIDs(allFactIDs)

    checkForDuplicateIDs(allEntryIDs.concat(allFactIDs).concat(planetIDs))

    // validate curiosity references and that all entries referred to as curiosities actually are curiosities
    const allCuriosityReferences = xmlDocs.map((doc, index) => [...doc.getElementsByTagName("Curiosity")].map(element => {
        return {element, referenceID: element.innerHTML, index}
    }))
    allCuriosityReferences
        .map(reference =>
            getElementByIDTag(allEntryIDs, reference.referenceID, `An <Entry> in file ${reference.index} has an invalid reference ID for <Curiosity>: "${reference.referenceID}". There is no <Entry> with the supplied ID.`)
                .getChildByTag(reference.element, "IsCuriosity", `Entry ${reference.referenceID} in file ${reference.index} is referred to as a curiosity by an event in file ${reference.index}, but does not have the <IsCuriosity/> tag.`)
        )

    // validate SourceID and Condition references
    validateReferences("SourceID", allEntryIDs, "Entry")
    validateReferences("Condition", allFactIDs, "ExploreFact or RumourFact")
    validateReferences("IgnoreMoreToExploreCondition", allFactIDs, "ExploreFact or RumourFact")


    // validate that all AltText tags have a <Condition> child
    allTagsOfTypeHaveChildOfType(xmlDocs, "AltText", "Condition")

    // validate that all <Entry> tags have a name
    allTagsOfTypeHaveChildOfType(xmlDocs, "Entry", "Name")
}

const validateReferences = (referencerType, possibleReferencees, referenceeType) => {
    const allReferences = xmlDocs.map((doc, index) => [...doc.getElementsByTagName("SourceID")].map(element => {
        return {element, referenceID: element.innerHTML, index}
    }))
    allReferences
        .map(reference =>
            getElementByIDTag(possibleReferencees, referenceID, `A <${referencerType}> tag in file ${reference.index} has an invalid reference id: "${reference.referenceID}". There is no <${referenceeType}> with the supplied ID.`)
        )
}

const allTagsOfTypeHaveChildOfType = (xmlDocs, tagType, childType) => {
    xmlDocs.map((doc, index) =>
        [...doc.getElementsByTagName(tagType)]
            .map((element, jndex) =>
                element.getChildByTag(element, childType, `<${tagType}> tag #${jndex} in file ${index} has no <${childType}> tag.`)
            )
    )
}

const checkForDuplicateIDs = (ids) => {
    const unique = {}
    const duplicate = {}

    for (const id of ids) {
        if (unique[id.id]) {
            if (duplicate[id.id]) duplicate[id.id].push(id.docIndex)
            else duplicate[id.id] = [unique[id.id], id.docIndex]
        } else {
            unique[id.id] = id.docIndex
        }
    }

    let duplications = ""
    for (const [id, docIndices] of Object.entries(duplicate)) {
        duplications += `${id} occurs in the following documents: ${docIndices.join(', ')}\n`
    }

    if (duplications !== "") throw new Error("Duplicate IDs were found:\n" + duplications)

    
}

const getAllIDsForTagType = (xmlDocs, tagType) => {
    const allIDs =
        xmlDocs
            .map((doc, index) =>
                [...doc.getElementsByTagName(tagType)].map((element, jndex) => {
                        const id = getChildByTag(
                            element,
                            "ID",
                            `Document ${index}'s <${tagType}> #${jndex} is missing its <ID>.`
                        ).innerHTML

                        return {
                            id,
                            docIndex: index,
                            element
                        }
                    }
                )
            )
            .flat()

    return allIDs
}

const getElementByIDTag = (idsList, id, errMsg) => {
    const possibleElements = idsList.filter(idMeta => idMeta.id === id).map(idMeta => idMeta.element)
    if (possibleElements === null || possibleElements.length === 0) {
        throw new Error(errMsg);
    }

    return possibleElements[0]
}

const getChildByTag = (xmlElement, tagName, errMsg) => {
    const possibleChildren = [...xmlElement.children].filter(element => element.tagName === tagName)
    if (possibleChildren === null || possibleChildren.length === 0) {
        throw new Error(errMsg);
    }

    return possibleChildren[0]
}


// Begin Idiot's Section (dear god no)

$(document).ready(() => {
    const fileSelector = $("#file-select")[0];
    const validationButton = $("#validate-button");
    const buttonText = $("#validate-button .text");
    const buttonSpinner = $("#validate-button .loading-icon");
    const errorBox = $("#error-box");
    const errorIcon = $("#error-box #error-icon");
    const errorText = $("#error-box #error-text");

    const setIsLoading = (isLoading) => {
        buttonText.toggleClass("d-none", isLoading);
        buttonSpinner.toggleClass("d-none", !isLoading);
    }
    
    const setError = (isError, message) => {
        errorBox.toggleClass("alert-secondary", false);
        errorIcon.toggleClass("bi-dash-circle", false);
        errorBox.toggleClass("alert-danger", isError);
        errorIcon.toggleClass("bi-exclamation-circle", isError);
        errorBox.toggleClass("alert-success", !isError);
        errorIcon.toggleClass("bi-check-circle", !isError);
        errorText.text(message);
    }
    
    fileSelector.addEventListener("change", (e) => {
        validationButton.prop("disabled", e.target.files.length === 0);
    });

    validationButton.click(async () => {
        setIsLoading(true);
        const allFiles = [];
        for (let file of fileSelector.files) {
            allFiles.push(await file.text());
        }
        const xmlDocs = parse(allFiles);
        try {
            validate(xmlDocs);
            setError(false, "No Issues Found!")
        }
        catch (e) {
            setError(true, e);
        }
        finally {
            setIsLoading(false);
        }
    });
});