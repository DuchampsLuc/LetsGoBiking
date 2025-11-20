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
let myTO;
const input =this.shadowRoot.querySelector("input");
let divResults=this.shadowRoot.querySelector("div");
input.addEventListener("keyup",function(e){
    
    if (myTO) clearTimeout(myTO);
    myTO=setTimeout(async()=>
    {   const inputContent=input.value;
        if (inputContent.length>2){
                const response = await fetch('https://api-adresse.data.gouv.fr/search/?q='+inputContent+'&limit=5');
                const data=await response.json();
                const result=data.features.map(feature=>feature.properties.label)
                console.log(result);
        }   
        },500)
        
})
}
static get observedAttributes() { return ["name"]; }

attributeChangedCallback(key, oldValue, newValue) { this[key] = newValue; }
}


customElements.define("auto-complete", AutoCompleteComponent);