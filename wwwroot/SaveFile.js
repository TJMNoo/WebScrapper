function FileSaveAs(filename, fileContent) {
    var link = document.createElement('a');
    link.download = filename;
    link.href = "data:text/plain;charset=utf-8," + encodeURIComponent(fileContent)
    document.body.appendChild(link);
    link.click();
    document.body.removeChild(link);
}

function consoleLog(message) {
    console.log(message);
}

function startSelector() {
    document.getElementById('selector').addEventListener('click', () => {
        let element = event.target;
        document.getElementById('selector-results').append(element.innerHTML);
        event.target.style = 'border: 2px solid red';
        event.preventDefault();
        console.log("target", element);
    })
}
