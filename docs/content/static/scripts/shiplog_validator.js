// Clay made all of this

// I wish this was TypeScript. The code would be so much cleaner and I wouldn't need so many "this returns objects of form X" comments ;-;

// filesContents is an array of the contents of the xml files to validate with
// throws and exception if the files are not valid xml
const parse = (filesContents) => {
	const parser = new DOMParser()
	const xmlDocs = filesContents.map((contents, index) => 
		{
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

    // validate that all <Entry> tags have a name
    allTagsOfTypeHaveChildOfType(xmlDocs, "Entry", "Name")

	// validate curiosity references and that all entries referred to as curiosities actually are curiosities
    validateReferences("Curiosity", allEntryIDs, "Entry")
        .map(referenceConection => {
            const curiosityEntry = referenceConection.referencedElement

            // get the ID of the Entry that should be a curiosity, for use in error reporting
            const curiosityEntryId = reference.referenceID
            
            // ensure that this Entry actually is marked as a curiosity
            getChildByTag(curiosityEntry, "IsCuriosity", `Entry id ${curiosityEntryId} is referred to as a curiosity by an <Entry> in file ${reference.index}, but does not have the <IsCuriosity/> tag.`)
        })

	// validate SourceID and Condition references
	validateReferences("SourceID", allEntryIDs, "Entry")
	validateReferences("Condition", allFactIDs, "ExploreFact or RumourFact")
	validateReferences("IgnoreMoreToExploreCondition", allFactIDs, "ExploreFact or RumourFact")
	
	
	// validate that all AltText tags have a <Condition> child
	allTagsOfTypeHaveChildOfType(xmlDocs, "AltText", "Condition")
}

const validateReferences = (referencerType, possibleReferencees, referenceeType) => {
    // allReferences is an array of all <referencerType> tags: 
    // [
    //     {
    //         element: instance of <referencerType>, 
    //         referenceID: the text between `element` and its matching closing tag, 
    //         index: the index of the xml document element is from
    //     }
    // ]
	const allReferences = xmlDocs.map(
		(doc, index) => 
		[...doc.getElementsByTagName(referencerType)]
		.map(element => { 
				return {element, referenceID: element.innerHTML, index} 
			}
		)
	)
	.flat()

    // the actual validation
	const tagsReferenced = allReferences
	.map(reference => {                                                // for each <referencerType> tag
            const referencedElement = getElementByIDTag(               // get the tag with the ID it references, where the id it references is <referencerType>THIS IS THE ID</referencerType>
                possibleReferencees,                                   // note: for most uses of this function, we don't do anything with the referenced tag, we're just making sure it exists, and throwing an error if it doesn't.
                referenceID, 
                `A <${referencerType}> tag in file ${reference.index} has an invalid reference id: "${reference.referenceID}". There is no <${referenceeType}> with the supplied ID.`
            )

            return {referencedElement, reference}
        }
	)

    // return the list of referenced tags and the referencepointing to them in case further validation is needed
    // return value type:
    // [
    //      {
    //          referencedElement: the tag being referenced, eg an <Entry>
    //          reference: {
    //              element: the tag doing the referencing, 
    //              referenceID: the id of the tag being referenced, 
    //              index: which xml file contains the tag doing the referencing
    //          }
    //      }
    // ]
    //
    //
    return tagsReferenced
}

const allTagsOfTypeHaveChildOfType = (xmlDocs, tagType, childType) => {
	xmlDocs.map((doc, index) =>                // for each document,
		[...doc.getElementsByTagName(tagType)] // get all <tagType> tags in this document
		.map((element, jndex) =>               // for each <tagType> in this document
			element.getChildByTag(             // make sure this tag has at least one <childType> tag as a child
				element, 
				childType, 
				`<${tagType}> tag #${jndex} in file ${index} has no <${childType}> tag.`
			)
		)
	)
}

const checkForDuplicateIDs = (ids) => {
    // ids is an array returned from getAllIDsForTagType

	const unique = {}     // a dictionary of { ID: [index of the doc a tag with this id appears in, ...] }
	const duplicate = {}  // same as the above. note: the arrays may contain duplicates, ie { "A_COOL_ROCK": [2, 2, 5, 2] }, meaning an element with id "A_COOL_ROCK" appears 3 times in doc #2 and once in doc #5

	for (const id of ids) {
		if (unique[id.id]) {
			if (duplicate[id.id]) duplicate[id.id].push(id.docIndex)
			else duplicate[id.id] = [unique[id.id], id.docIndex]
		} else {
			unique[id.id] = id.docIndex
		}
	}

	const duplicationsMessage = ""
	for (const [id, docIndices] of Object.entries(duplicate)) {
		duplicationsMessage += `${id} occurs in the following documents: ${docIndices.join(', ')}\n`
	}

	if (duplicationsMessage !== "") throw new Error("Duplicate IDs were found:\n" + duplicationsMessage)

	return
}

const getAllIDsForTagType = (xmlDocs, tagType) => {
	const allIDs = xmlDocs
		.map((doc, index) =>                             // for each document,
			[...doc.getElementsByTagName(tagType)]       // get all <tagType> tags
			.map((element, jndex) => {                   // for each <tagType>
				const id = getChildByTag(                // get its child <ID> tag
					element, 
					"ID", 
					`Document ${index}'s <${tagType}> #${jndex} is missing its <ID>.`
				).innerHTML
				
				return {                                 // and return {
					id,                                  //     id: text between its child <ID> tag and the matching </ID>, 
					docIndex: index,                     //     docIndex: index of the document,
					element                              //     element: the <tagType> element (aka the <tagType> element with ID `id`)
				}                                        // }
			})
		)
		.flat()

    // returns a list of: [
    //     {
    //         id: text between its child <ID> tag and the matching </ID>, 
    //         docIndex: index of the document,
    //         element: the <tagType> element (aka the <tagType> element with ID `id`)
    //     }
    // ]
	return allIDs
}

const getElementByIDTag = (idsList, id, errMsg) => {
    // ids list is a list returned by getAllIDsForTagType
    // id is a string
    // errMsg is also a string

	const possibleElements = idsList.filter(idMeta => idMeta.id === id).map(idMeta => idMeta.element)
	if (possibleElements === null || possibleElements.length === 0) {
		throw new Error(errMsg);
	}

	return possibleElements[0]
}

const getChildByTag = (xmlElement, tagName, errMsg) => {
    // gets the first <tagName> child of xmlElement 

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