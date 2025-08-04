// Questions-specific JavaScript functionality

// Questions page specific functions
function initializeQuestionsPage() {
    // Sort functionality
    const sortSelect = document.getElementById('sortSelect');
    if (sortSelect) {
        sortSelect.addEventListener('change', function() {
            const url = new URL(window.location);
            url.searchParams.set('sort', this.value);
            window.location.href = url.toString();
        });
    }
    
    // Tag filtering
    document.querySelectorAll('.tag-filter').forEach(tag => {
        tag.addEventListener('click', function(e) {
            e.preventDefault();
            const tagName = this.getAttribute('data-tag');
            const url = new URL(window.location);
            url.searchParams.set('tag', tagName);
            url.searchParams.delete('page'); // Reset pagination
            window.location.href = url.toString();
        });
    });
}

// Question details page specific functions
function initializeQuestionDetails() {
    // Scroll to answer if hash is present
    if (window.location.hash.startsWith('#answer-')) {
        const answerId = window.location.hash.replace('#answer-', '');
        const answerElement = document.getElementById(`answer-${answerId}`);
        if (answerElement) {
            answerElement.scrollIntoView({ behavior: 'smooth' });
            answerElement.classList.add('highlight-answer');
        }
    }
    
    // Share functionality
    document.querySelectorAll('.share-btn').forEach(btn => {
        btn.addEventListener('click', function() {
            const url = window.location.href;
            if (navigator.share) {
                navigator.share({
                    title: document.title,
                    url: url
                });
            } else {
                // Fallback: copy to clipboard
                navigator.clipboard.writeText(url).then(() => {
                    showToast('URL copied to clipboard!', 'success');
                });
            }
        });
    });
}

// Ask question page specific functions
function initializeAskQuestion() {
    const titleInput = document.getElementById('Title');
    const tagsInput = document.getElementById('Tags');
    
    // Title character counter and validation
    if (titleInput) {
        const titleCounter = document.createElement('small');
        titleCounter.className = 'text-muted';
        titleInput.parentNode.appendChild(titleCounter);
        
        function updateTitleCounter() {
            const length = titleInput.value.length;
            const maxLength = titleInput.getAttribute('maxlength') || 200;
            const remaining = maxLength - length;
            
            titleCounter.textContent = `${remaining} characters remaining`;
            titleCounter.className = remaining < 20 ? 'text-warning' : 'text-muted';
            
            // Add validation styling
            if (length < 10) {
                titleInput.classList.add('is-invalid');
                titleInput.classList.remove('is-valid');
            } else {
                titleInput.classList.add('is-valid');
                titleInput.classList.remove('is-invalid');
            }
        }
        
        titleInput.addEventListener('input', updateTitleCounter);
        updateTitleCounter();
    }
    
    // Tags input enhancement
    if (tagsInput) {
        tagsInput.addEventListener('input', function() {
            const tags = this.value.split(',').map(tag => tag.trim()).filter(tag => tag.length > 0);
            
            // Limit to 5 tags
            if (tags.length > 5) {
                const limitedTags = tags.slice(0, 5);
                this.value = limitedTags.join(', ');
                showToast('Maximum 5 tags allowed', 'warning');
            }
            
            // Validate tags
            const validTags = tags.every(tag => tag.length >= 2 && tag.length <= 25);
            if (validTags && tags.length > 0) {
                this.classList.add('is-valid');
                this.classList.remove('is-invalid');
            } else if (tags.length > 0) {
                this.classList.add('is-invalid');
                this.classList.remove('is-valid');
            } else {
                this.classList.remove('is-valid', 'is-invalid');
            }
        });
        
        // Tag suggestions
        const tagSuggestions = ['C#', 'JavaScript', 'ASP.NET', 'SQL', 'HTML', 'CSS', 'Python', 'React', 'Angular', 'Vue.js'];
        
        // Create suggestions dropdown
        const suggestionsDiv = document.createElement('div');
        suggestionsDiv.className = 'tag-suggestions mt-2';
        suggestionsDiv.style.display = 'none';
        tagsInput.parentNode.appendChild(suggestionsDiv);
        
        tagsInput.addEventListener('focus', function() {
            showTagSuggestions();
        });
        
        tagsInput.addEventListener('blur', function() {
            // Delay hiding to allow clicking suggestions
            setTimeout(() => {
                suggestionsDiv.style.display = 'none';
            }, 200);
        });
        
        function showTagSuggestions() {
            const currentTags = tagsInput.value.split(',').map(tag => tag.trim().toLowerCase());
            const availableTags = tagSuggestions.filter(tag => 
                !currentTags.includes(tag.toLowerCase())
            );
            
            if (availableTags.length > 0) {
                suggestionsDiv.innerHTML = availableTags.map(tag => 
                    `<button type="button" class="btn btn-sm btn-outline-secondary me-1 mb-1 tag-suggestion" data-tag="${tag}">${tag}</button>`
                ).join('');
                suggestionsDiv.style.display = 'block';
                
                // Add click handlers
                suggestionsDiv.querySelectorAll('.tag-suggestion').forEach(btn => {
                    btn.addEventListener('click', function() {
                        addTag(this.getAttribute('data-tag'));
                    });
                });
            }
        }
        
        function addTag(tag) {
            const currentValue = tagsInput.value.trim();
            if (currentValue === '') {
                tagsInput.value = tag;
            } else {
                const tags = currentValue.split(',').map(t => t.trim());
                if (!tags.includes(tag)) {
                    tags.push(tag);
                    tagsInput.value = tags.join(', ');
                }
            }
            tagsInput.dispatchEvent(new Event('input'));
            tagsInput.focus();
        }
    }
    
    // Form validation before submit
    const form = document.querySelector('form[asp-action="Ask"]');
    if (form) {
        form.addEventListener('submit', function(e) {
            const title = document.getElementById('Title').value.trim();
            const description = document.getElementById('Description').value.trim();
            
            if (title.length < 10) {
                e.preventDefault();
                showToast('Title must be at least 10 characters long', 'error');
                document.getElementById('Title').focus();
                return;
            }
            
            if (description.length < 20) {
                e.preventDefault();
                showToast('Description must be at least 20 characters long', 'error');
                document.getElementById('Description').focus();
                return;
            }
        });
    }
}

// Real-time search functionality
function initializeRealTimeSearch() {
    const searchInput = document.querySelector('input[name="search"]');
    if (searchInput && window.location.pathname === '/Questions') {
        let searchTimeout;
        
        searchInput.addEventListener('input', function() {
            clearTimeout(searchTimeout);
            searchTimeout = setTimeout(() => {
                if (this.value.length >= 3 || this.value.length === 0) {
                    // Perform search
                    const url = new URL(window.location);
                    if (this.value.trim()) {
                        url.searchParams.set('search', this.value.trim());
                    } else {
                        url.searchParams.delete('search');
                    }
                    url.searchParams.delete('page'); // Reset pagination
                    
                    // Update URL without page reload (optional)
                    // window.history.pushState({}, '', url.toString());
                    
                    // For now, just reload with new search
                    window.location.href = url.toString();
                }
            }, 500);
        });
    }
}

// Markdown preview functionality
function initializeMarkdownPreview() {
    const descriptionTextarea = document.getElementById('Description');
    if (descriptionTextarea) {
        // Add preview toggle button
        const previewBtn = document.createElement('button');
        previewBtn.type = 'button';
        previewBtn.className = 'btn btn-sm btn-outline-secondary mb-2';
        previewBtn.innerHTML = '<i class="fas fa-eye me-1"></i>Preview';
        
        const previewDiv = document.createElement('div');
        previewDiv.className = 'markdown-preview border rounded p-3 mb-2';
        previewDiv.style.display = 'none';
        
        descriptionTextarea.parentNode.insertBefore(previewBtn, descriptionTextarea);
        descriptionTextarea.parentNode.insertBefore(previewDiv, descriptionTextarea);
        
        let isPreviewMode = false;
        
        previewBtn.addEventListener('click', function() {
            if (isPreviewMode) {
                // Switch to edit mode
                descriptionTextarea.style.display = 'block';
                previewDiv.style.display = 'none';
                this.innerHTML = '<i class="fas fa-eye me-1"></i>Preview';
                isPreviewMode = false;
            } else {
                // Switch to preview mode
                const content = descriptionTextarea.value;
                previewDiv.innerHTML = formatMarkdown(content);
                descriptionTextarea.style.display = 'none';
                previewDiv.style.display = 'block';
                this.innerHTML = '<i class="fas fa-edit me-1"></i>Edit';
                isPreviewMode = true;
                
                // Highlight code in preview
                hljs.highlightAll();
            }
        });
    }
}

// Simple markdown formatter
function formatMarkdown(text) {
    if (!text) return '';
    
    // Convert markdown to HTML (basic implementation)
    let html = text
        // Code blocks
        .replace(/```(\w+)?\n([\s\S]*?)```/g, '<pre><code class="language-$1">$2</code></pre>')
        // Inline code
        .replace(/`([^`]+)`/g, '<code>$1</code>')
        // Bold
        .replace(/\*\*(.*?)\*\*/g, '<strong>$1</strong>')
        // Italic
        .replace(/\*(.*?)\*/g, '<em>$1</em>')
        // Line breaks
        .replace(/\n/g, '<br>');
    
    return html;
}

// Initialize based on current page
document.addEventListener('DOMContentLoaded', function() {
    const path = window.location.pathname;
    
    if (path === '/Questions' || path === '/Questions/Index') {
        initializeQuestionsPage();
        initializeRealTimeSearch();
    } else if (path.startsWith('/Questions/Details/')) {
        initializeQuestionDetails();
    } else if (path === '/Questions/Ask') {
        initializeAskQuestion();
        initializeMarkdownPreview();
    }
});
