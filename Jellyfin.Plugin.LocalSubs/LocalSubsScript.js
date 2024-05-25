const LocalSubsGuid = '7de4aa03-f418-4e1c-a8ba-08ccecba4ab5';

function controller(view, params) {
    function byId(id) {
        return view.querySelector('#' + id);
    }

    function clearChildren(elem) {
        if (elem) {
            while (elem.firstChild) elem.removeChild(elem.lastChild);
        }
    }

    function generateTemplates(templates) {
        if (!templates || templates.length < 1) {
            const span = document.createElement('span');
            span.textContent = 'Empty';
            const holder = byId('LocalSubsFormTemplates');
            clearChildren(holder);
            holder.appendChild(span);
            return;
        }
        const items = document.createElement('div');
        items.dataset.role = 'controlgroup';
        for (const template of templates) {
            const checkbox = document.createElement('input');
            checkbox.type = 'checkbox';
            checkbox.classList.add('checkbox', 'checkboxContainer-withDescription', 'templateCheckbox');
            checkbox.setAttribute('is', 'emby-checkbox');
            checkbox.checked = false;
            checkbox.dataset.mini = true;
            checkbox.dataset.template = template;
            checkbox.name = template;
            const span = document.createElement('span');
            span.textContent = template;
            const label = document.createElement('label');
            label.appendChild(checkbox);
            label.appendChild(span);
            const container = document.createElement('div');
            container.classList.add('checkboxContainer');
            items.appendChild(label);
        }
        const holder = byId('LocalSubsFormTemplates');
        clearChildren(holder);
        holder.appendChild(items);
    }

    function refreshTemplates() {
        Dashboard.showLoadingMsg();
        const page = this;
        ApiClient.getPluginConfiguration(LocalSubsGuid).then(function (config) {
            console.debug('refreshTemplates:getPluginConfiguration', config);
            generateTemplates(config.Templates);
            Dashboard.hideLoadingMsg();
        });
    }

    view.addEventListener('viewshow', function (evt) {
        evt.preventDefault();
        refreshTemplates();
    });

    byId('LocalSubsFormAdd').addEventListener('click', function (evt) {
        evt.preventDefault();
        const template = byId('LocalSubsFormNewTemplate').value ? byId('LocalSubsFormNewTemplate').value.trim() : undefined;
        if (!template) return;
        Dashboard.showLoadingMsg();
        const data = JSON.stringify({ Template: template });
        const url = ApiClient.getUrl('Jellyfin.Plugin.LocalSubs/AddTemplate');
        ApiClient.ajax({ type: 'POST', url, data, contentType: 'application/json' }).then(function (res) {
            console.debug('AddTemplate', res);
            refreshTemplates();
            Dashboard.hideLoadingMsg();
        }).catch(function (err) {
            Dashboard.processErrorResponse({ statusText: 'Failed' });
            refreshTemplates();
            Dashboard.hideLoadingMsg();
        });
    });

    byId('LocalSubsFormDelete').addEventListener('click', function (evt) {
        evt.preventDefault();
        const templates = [];
        for (const checkbox of view.querySelectorAll('input.templateCheckbox')) {
            if (checkbox.checked) {
                templates.push(checkbox.dataset.template);
            }
        }
        if (templates.length < 1) return;
        Dashboard.showLoadingMsg();
        const data = JSON.stringify({ Templates: templates });
        const url = ApiClient.getUrl('Jellyfin.Plugin.LocalSubs/DeleteTemplates');
        ApiClient.ajax({ type: 'POST', url, data, contentType: 'application/json' }).then(function (res) {
            console.debug('DeleteTemplates', res);
            refreshTemplates();
            Dashboard.hideLoadingMsg();
        }).catch(function (err) {
            Dashboard.processErrorResponse({ statusText: 'Failed' });
            refreshTemplates();
            Dashboard.hideLoadingMsg();
        });
    });

    byId('LocalSubsFormReset').addEventListener('click', function (evt) {
        evt.preventDefault();
        Dashboard.showLoadingMsg();
        const url = ApiClient.getUrl('Jellyfin.Plugin.LocalSubs/ResetTemplates');
        ApiClient.ajax({ type: 'POST', url }).then(function (res) {
            console.debug('ResetTemplates', res);
            refreshTemplates();
            Dashboard.hideLoadingMsg();
        }).catch(function (err) {
            Dashboard.processErrorResponse({ statusText: 'Failed' });
            refreshTemplates();
            Dashboard.hideLoadingMsg();
        });
    });
};

export default controller;
