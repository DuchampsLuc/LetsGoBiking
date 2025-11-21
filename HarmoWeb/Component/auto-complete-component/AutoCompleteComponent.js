class AutoCompleteComponent extends HTMLElement {
constructor() {
    super();
    let shadowRoot = this.attachShadow({ mode: "open" });
}
async getResult(){
    
}
async connectedCallback() {
const response = await fetch('Component/auto-complete-component/autoCompleteComponent.html');
const html = await response.text();
const template = new DOMParser().parseFromString(html, "text/html").querySelector("template");
if (template) {
this.shadowRoot.appendChild(template.content.cloneNode(true));
}
const name=this.getAttribute("name");
this.shadowRoot.querySelector("#name").textContent=name;
this.shadowRoot.querySelector("#results")

let myTO;
const input =this.shadowRoot.querySelector("input");
let divResults=this.shadowRoot.querySelector("div");
input.addEventListener("keyup",function(e){
    
    if (myTO) clearTimeout(myTO);
    myTO=setTimeout(async()=>
    {   const inputContent=input.value;
        if (inputContent.length>2){
                console.log("Fetching for "+inputContent);

                const response = await fetch('http://localhost:8733/Design_Time_Addresses/BackendBiking/Service1/GetAdresse?adresse=' + inputContent);
                const data=await response.json();
                const resultData = JSON.parse(data.GetAdresseResult);
                const result = resultData.features.map(f => f.properties.label);
                //console.log(data.GetAdresseResult);
                divResults.innerHTML = result.map(i => `<div class="item">${i}</div>`).join("");
                console.log(result);
                console.log(this.shadowRoot.querySelector("#results"));

        }   
        },500)
        
})
}
static get observedAttributes() { return ["name"]; }

attributeChangedCallback(key, oldValue, newValue) { this[key] = newValue; }
}


customElements.define("auto-complete", AutoCompleteComponent);