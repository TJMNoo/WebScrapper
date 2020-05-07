let selected = []
let selectedElements = []

function startSelector(dotnetInstance) {
    var frame = document.getElementById('selector')
    var frameDoc = frame.contentDocument ? frame.contentDocument : frame.contentWindow.document
    frameDoc.addEventListener('click', () => {
        event.preventDefault()
        let element = event.target
        let index = selectedElements.findIndex(el => el == element)
        if (index != -1) {
            element.style = selected[index].style
            selected.splice(index, 1)
            selectedElements.splice(index, 1)
        }
        else {
            selectedElements.push(element)
            selected.push(element.outerHTML)
            element.style = 'border: 2px solid MediumTurquoise !important; background-color: PaleTurquoise !important'
        }
        dotnetInstance.invokeMethodAsync('UpdateSelectedElements')
    })
}

function removeSelected(element) {
    let index = selected.findIndex(el => el == element)
    selectedElements[index].style = selected[index].style
    selected.splice(index, 1)
    selectedElements.splice(index, 1)
}

function getSelected() {
    return selected
}

function resetSelected() {
    selected = []
    selectedElements = []
}

function consoleLog(message) {
    console.log(message)
}
