let selected = []
let selectedElements = []

function startSelector(dotnetInstance) {
    document.getElementById('selector').contentWindow.document.addEventListener('click', () => {
        event.preventDefault()

        console.log('target', event)

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
            element.style = 'border: 2px solid red !important; background-color: darkred !important'
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

function consoleLog(message) {
    console.log(message)
}

