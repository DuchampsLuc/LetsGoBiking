class MeteoButton extends HTMLElement {
    constructor() {
        super();
        this.attachShadow({ mode: "open" });
    }

    async connectedCallback() {
        const t = await fetch("Component/notification-component/notification.html");
        const html = await t.text();
        const template = new DOMParser()
            .parseFromString(html, "text/html")
            .querySelector("template");

        this.shadowRoot.appendChild(template.content.cloneNode(true));

        this.resultDiv = this.shadowRoot.querySelector("#result");
        this.button = this.shadowRoot.querySelector("#meteoBtn");
        this.itineraireDiv = this.shadowRoot.querySelector("#itineraire");
        this.currentStepIndex = 0;
        this.allSteps = [];

        // STOMP client
        this.client = new StompJs.Client({
            brokerURL: "ws://localhost:61614/",
            reconnectDelay: 3000
        });

        this.client.onConnect = () => {
            console.log("STOMP connecté");
            this.client.subscribe("/queue/meteo", msg => {
                const raw = msg.body;
                let parsed;
                try {
                    parsed = JSON.parse(raw);
                } catch (e) {
                    console.warn("Message meteo non JSON, corps brut:", raw);
                    // Fallback: créer objet minimal
                    parsed = { raw };
                }
                if (!parsed.current_weather && parsed.raw) {
                    // Tentative extraction température simple si format 'temp:xx' etc.
                    const tempMatch = /(?:temp(?:erature)?)[^0-9]*([0-9]+(?:\.[0-9]+)?)/i.exec(parsed.raw);
                    if (tempMatch) {
                        parsed.current_weather = { temperature: parseFloat(tempMatch[1]), windspeed: null };
                    }
                }
                this.showMeteo(parsed);
            });
        };

        this.client.activate();

        // Click
        this.button.addEventListener("click", () => this.demanderMeteo());
        
        // Écouter les changements d'itinéraire
        window.addEventListener('routeUpdated', (e) => {
            if (e.detail && e.detail.route) {
                this.displayRoute(e.detail.route);
            }
        });
    }

    demanderMeteo() {
        if (!window.departadress) {
            alert("Aucune position sélectionnée");
            return;
        }
        const sourceAdresse = (window.departadressRaw || window.departadress || "").trim();
        const rawAdresse = sourceAdresse;
        if (!rawAdresse) {
            console.warn("departadress est vide après trim.");
            return;
        }
        const adresseEncoded = encodeURIComponent(rawAdresse);
        const url = `http://localhost:8733/Design_Time_Addresses/BackendBiking/Service1/GetNotification?adresse=${adresseEncoded}`;
        console.log("[GetNotification] URL appelée:", url, "adresse source:", rawAdresse);
        fetch(url)
            .then(async r => {
                const contentType = r.headers.get('content-type') || '';
                const bodyText = await r.text();
                if (!r.ok) {
                    console.error("GetNotification HTTP", r.status, bodyText);
                    throw new Error(`GetNotification failed ${r.status}`);
                }
                let parsed;
                try {
                    parsed = contentType.includes('application/json') ? JSON.parse(bodyText) : bodyText;
                } catch (e) {
                    console.warn("JSON parse error:", e, bodyText);
                    parsed = bodyText;
                }
                console.log("GetNotification réponse brute:", parsed);
                if (parsed && parsed.GetNotificationResult) {
                    console.log("Résultat notification:", parsed.GetNotificationResult);
                }
                console.log("Demande météo envoyée au serveur (message ActiveMQ devrait être produit)");
            })
            .catch(err => {
                console.error("Erreur fetch GetNotification:", err);
                alert("Échec de la demande météo (voir console)");
            });
    }

    removeAccents(str) {
    return str.normalize("NFD").replace(/[\u0300-\u036f]/g, "");
        }

    showMeteo(data) {
        this.resultDiv.innerHTML = `
            <p><b>Température :</b> ${data.current_weather.temperature}°C</p>
            <p><b>Vent :</b> ${data.current_weather.windspeed} km/h</p>
        `;
    }

    displayRoute(routeData) {
        this.allSteps = [];
        routeData.segments.forEach(segment => {
            const mode = segment.mode;
            if (segment.route && segment.route.features && segment.route.features[0]) {
                const props = segment.route.features[0].properties;
                if (props.segments && props.segments[0] && props.segments[0].steps) {
                    props.segments[0].steps.forEach(step => {
                        this.allSteps.push({ ...step, mode });
                    });
                }
            }
        });
        this.currentStepIndex = 0;
        this.renderCurrentStep();
    }

    renderCurrentStep() {
        if (this.allSteps.length === 0) {
            this.itineraireDiv.innerHTML = '<p>Aucun itinéraire disponible</p>';
            return;
        }
        const step = this.allSteps[this.currentStepIndex];
        const distance = (step.distance / 1000).toFixed(2);
        const duration = Math.round(step.duration / 60);
        
        this.itineraireDiv.innerHTML = `
            <h4>Étape ${this.currentStepIndex + 1} / ${this.allSteps.length}</h4>
            <div class="step-content">
                <span class="step-mode mode-${step.mode}">${step.mode === 'walking' ? '🚶 Marche' : '🚴 Vélo'}</span>
                <div class="step-instruction">${step.instruction}</div>
                <div class="step-info">📏 ${distance} km • ⏱️ ${duration} min</div>
            </div>
            <div class="step-navigation">
                <button class="nav-btn" id="prevBtn" ${this.currentStepIndex === 0 ? 'disabled' : ''}>← Précédent</button>
                <span>${this.currentStepIndex + 1} / ${this.allSteps.length}</span>
                <button class="nav-btn" id="nextBtn" ${this.currentStepIndex === this.allSteps.length - 1 ? 'disabled' : ''}>Suivant →</button>
            </div>
        `;
        
        const prevBtn = this.shadowRoot.querySelector('#prevBtn');
        const nextBtn = this.shadowRoot.querySelector('#nextBtn');
        
        if (prevBtn) prevBtn.addEventListener('click', () => this.previousStep());
        if (nextBtn) nextBtn.addEventListener('click', () => this.nextStep());
    }

    previousStep() {
        if (this.currentStepIndex > 0) {
            this.currentStepIndex--;
            this.renderCurrentStep();
        }
    }

    nextStep() {
        if (this.currentStepIndex < this.allSteps.length - 1) {
            this.currentStepIndex++;
            this.renderCurrentStep();
        }
    }
}

customElements.define("meteo-button", MeteoButton);
