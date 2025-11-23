
        // MapComponent
        class MapComponent extends HTMLElement {
            constructor() {
                super();
                this.map = null;
            }

            connectedCallback() {
                this.innerHTML = `<div id="map"></div>`;

                // attendre que le navigateur ait calculé la taille
                requestAnimationFrame(() => this.initMap());
            }

            initMap() {
                const mapDiv = this.querySelector('#map');

                if (!window.L) {
                    console.error("Leaflet non chargé !");
                    return;
                }

                this.map = L.map(mapDiv).setView([48.8566, 2.3522], 6);

                L.tileLayer('https://{s}.tile.openstreetmap.org/{z}/{x}/{y}.png', {
                    attribution: '© OpenStreetMap contributors'
                }).addTo(this.map);

                window.Map = this.map;

                // recalculer la taille après rendu
                setTimeout(() => this.map.invalidateSize(), 200);
            }
        }

        customElements.define('map-component', MapComponent);

        // Placeholder pour auto-complete (exemple)
        class AutoComplete extends HTMLElement {
            connectedCallback() {
                this.innerHTML = `<input type="text" placeholder="${this.getAttribute('name')}">`;
            }
        }
        customElements.define('auto-complete', AutoComplete);

        // Placeholder header/footer
        class MyHeader extends HTMLElement {
            connectedCallback() { this.innerHTML = `<header style="border-bottom:2px solid black; padding:10px;">HEADER</header>`; }
        }
        customElements.define('my-header', MyHeader);

        class MyFooter extends HTMLElement {
            connectedCallback() { this.innerHTML = `<footer style="border-top:2px solid black; padding:10px;">FOOTER</footer>`; }
        }
        customElements.define('my-footer', MyFooter);