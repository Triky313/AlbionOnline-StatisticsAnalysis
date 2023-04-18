fetch("https://api.github.com/repos/Triky313/AlbionOnline-StatisticsAnalysis/releases")
  .then(response => response.json())
  .then(data => {
    const releases = data.sort((a, b) => {
      return new Date(b.created_at) - new Date(a.created_at);
    });
    
    const featuresRegex = /features|enhancements/i;
    const fixesRegex = /fixes|bug\sfixes/i;
    const changesRegex = /changes/i;
    
    const features = [];
    const fixes = [];
    const changes = [];
    
    releases.forEach(release => {
      const notes = release.body;
      
      if (featuresRegex.test(notes)) {
        features.push(release);
      } else if (fixesRegex.test(notes)) {
        fixes.push(release);
      } else if (changesRegex.test(notes)) {
        changes.push(release);
      }
    });
    
    const releasesDiv = document.getElementById("releases");
    
    // Erstelle HTML-Elemente für jede Kategorie
    const featuresHtml = createCategoryHtml("Features", features);
    const fixesHtml = createCategoryHtml("Fixed", fixes);
    const changesHtml = createCategoryHtml("Changes", changes);
    
    // Füge die HTML-Elemente in die Releases-Div ein
    releasesDiv.innerHTML = featuresHtml + fixesHtml + changesHtml;
  })
  .catch(error => {
    console.error(error);
  });

// Hilfsfunktion zum Erstellen von HTML-Elementen für jede Kategorie
function createCategoryHtml(category, releases) {
  let html = `<h2>${category}</h2>`;
  
  if (releases.length === 0) {
    html += "<p>No releases found.</p>";
  } else {
    html += "<ul>";
    
    releases.forEach(release => {
      const releaseUrl = release.html_url;
      const releaseName = release.name;
      
      html += `<li><a href="${releaseUrl}">${releaseName}</a></li>`;
    });
    
    html += "</ul>";
  }
  
  return html;
}