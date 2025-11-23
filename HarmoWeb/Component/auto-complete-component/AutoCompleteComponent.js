window.departadress = null;
window.arriveeadress = null;


class AutoCompleteComponent extends HTMLElement {
constructor() {
    super();
    let shadowRoot = this.attachShadow({ mode: "open" });
}
async getResult(){
    
}
async connectedCallback() {
    const self=this;
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
input.addEventListener("keyup",(e)=>{
    
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
                divResults.querySelectorAll('.item').forEach(item => {
                item.addEventListener('click', async () => {
                    const adresseChoisie = item.textContent;
                    console.log("Adresse choisie :", adresseChoisie);
                    const coordResponse = await fetch('http://localhost:8733/Design_Time_Addresses/BackendBiking/Service1/GetCoordonnees?adresse=' + encodeURIComponent(adresseChoisie));
                    const coordData = await coordResponse.json();
                    const { lat, lng } = JSON.parse(coordData.GetCoordonneesResult);;
                            // Supposons que ton serveur renvoie {lat: ..., lng: ...}
                    console.log("Coordonnées reçues :", coordData);
                    if (window.Map) {
                        L.marker([lat, lng]).addTo(window.Map)
                            .bindPopup(adresseChoisie)
                            .openPopup();

                        // Centrer la carte sur le marker
                        window.Map.setView([lat, lng], 15);
                    }

                    // Remplir l'input et vider les résultats
                    input.value = adresseChoisie;
                    divResults.innerHTML = "";
                    const name = self.getAttribute("name"); // "Depart" ou "Arrivée"
                    if (name === "Depart") {
                        window.departadress = removeAccents(adresseChoisie);
                        console.log("Départ défini à :", window.departadress);
                    } else if (name === "Arrivée") {
                        window.arriveeadress = removeAccents(adresseChoisie);
                        console.log("Arrivée définie à :", window.arriveeadress);
                    }

                    if (window.departadress && window.arriveeadress) {
                            console.log("Appel du serveur avec : ", window.departadress, window.arriveeadress);
                            const adresse=`http://localhost:8733/Design_Time_Addresses/BackendBiking/Service1/GetRoute?start=${window.departadress}&dest=${window.arriveeadress}`;
                            console.log("URL de l'itinéraire :", adresse);
                            //Exemple fetch vers ton serveur avec les deux points
                            const response = await fetch(adresse);
                            const itineraire = await response.json();
                            console.log("Itinéraire reçu : ", itineraire);
                            const routeData = JSON.parse(itineraire.GetRouteResult);
                            const coords = routeData.segments[0].route.features[0].geometry.coordinates;
                            const latlngs = coords.map(c => [c[1], c[0]]);
                            console.log("LatLngs de la route :", latlngs);

                            if (window.Map) {
                        // Supprime l'ancien itinéraire
                        if (window.currentRoute) {
                            window.currentRoute.forEach(layer => window.Map.removeLayer(layer));
                        }
                        window.currentRoute = [];

                        // Parcours chaque segment
                        routeData.segments.forEach(segment => {
                            const coords = segment.route.features[0].geometry.coordinates;
                            const latlngs = coords.map(c => [c[1], c[0]]);

                            // Définir la couleur selon le mode
                            const color = segment.mode === 'walking' ? 'green' : 'blue';

                            const polyline = L.polyline(latlngs, { color: color, weight: 5 }).addTo(window.Map);
                            window.currentRoute.push(polyline);
                        });

                        // Ajuste la vue sur le dernier segment
                        const allBounds = L.latLngBounds(
                            window.currentRoute.flatMap(p => p.getLatLngs())
                        );
                        window.Map.fitBounds(allBounds);
                    }
                        

                    }

                    //const { lat, lng } = coordData;
                
                });});
                console.log(result);
                console.log(this.shadowRoot.querySelector("#results"));

        }   
        },500)
        
})
}
static get observedAttributes() { return ["name"]; }

attributeChangedCallback(key, oldValue, newValue) { this[key] = newValue; }
}

function removeAccents(str) {
    return str.normalize("NFD").replace(/[\u0300-\u036f]/g, "");
}


customElements.define("auto-complete", AutoCompleteComponent);