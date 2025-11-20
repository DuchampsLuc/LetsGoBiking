class MonComposant extends HTMLElement {
constructor() {
    super();
    let shadowRoot = this.attachShadow({ mode: "open" });
}

async connectedCallback() {
const response = await fetch('Component/footer/footercomponent.html');
const html = await response.text();
const template = new DOMParser().parseFromString(html, "text/html").querySelector("template");
if (template) {
this.shadowRoot.appendChild(template.content.cloneNode(true));
}

}
}

customElements.define("my-footer", MonComposant);