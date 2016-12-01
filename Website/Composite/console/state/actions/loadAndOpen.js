import { loadPageDef } from 'console/state/actions/pageDefs.js';
import { loadValues } from 'console/state/actions/values.js';
import { openPage, setPage } from 'console/state/reducers/layout.js';
import { getLogDates, getLogPage } from 'console/state/actions/logs.js';
import { getProviderPage } from 'console/state/actions/fetchFromProvider.js';
import { subscribe } from 'console/state/observers.js';

const actionList = {
	getLogDates,
	getLogPage
};

const pageLoaders = {
	document: (pageName, getState, dispatch) => {
		let pageDef = getState().getIn(['pageDefs', pageName]);
		let loading = [
			dispatch(loadValues(pageName))
		];
		// Run through toolbars, load options
		pageDef.get('toolbars').forEach(toolbarName => {
			let toolbarDef = getState().getIn(['toolbarDefs', toolbarName]);
			toolbarDef.get('items').forEach(itemName => {
				let itemDef = getState().getIn(['itemDefs', itemName]);
				if (itemDef.get('type') === 'select' && itemDef.get('optionLoader')) {
					loading.push(dispatch(actionList[itemDef.get('optionLoader')](itemName)));
				}
			});
		});
		let tabs = pageDef && pageDef.get('tabs') ?
			pageDef.get('tabs').toArray() :
			[];
		tabs.forEach(tabName =>
			loading.push(dispatch(loadTabValues(tabName)))
		);
		return Promise.all(loading);
	},
	dialogPageShim: (pageName, getState, dispatch) => {
		let dialogName = getState().getIn(['pageDefs', pageName, 'dialog']);
		let dialogDef = getState().getIn(['dialogDefs', dialogName]);
		if (dialogDef.get('type') === 'palette') {
			let provider = dialogDef.getIn(['providers', 'elementSource']).toJS();
			return dispatch(getProviderPage(provider, dialogName, dialogDef.get('context')));
		} else {
			return Promise.resolve();
		}
	}
};

export function loadAndOpenPage(pageName) {
	return (dispatch, getState) => {
		let loadDef = getState().getIn(['pageDefs', pageName]) ?
			Promise.resolve(null) :
			dispatch(loadPageDef(pageName));
		return Promise.all([
			loadDef.then(() => {
				let pageDef = getState().getIn(['pageDefs', pageName]);
				// Load any options specified
				let tabs = pageDef && pageDef.get('tabs') ?
					pageDef.get('tabs').toArray() :
					[];
				let loading = [];
				dispatch(openPage(pageName, tabs));
				dispatch(setPage(pageName));
				let loader = pageLoaders[pageDef.get('type')];
				if (loader) {
					loading.push(loader(pageName, getState, dispatch));
				}
				return Promise.all(loading);
			})
		]);
	};
}

const tabLoaders = {
	log: (tabName, getState, dispatch) => {
		let dateSelectorName = getState().getIn(['tabDefs', tabName, 'logPageName']);
		let loadLogs = () => {
			let date = getState().getIn(['options', 'values', dateSelectorName]) ||
				getState().getIn(['options', 'lists', dateSelectorName, 0, 'value']);
			if (!getState().getIn(['logs', tabName, date])) {
				dispatch(actionList.getLogPage(tabName, date));
			}
		};
		subscribe(['options', 'values', dateSelectorName], loadLogs);
		return loadLogs();
	}
};

export function loadTabValues(tabName) {
	return (dispatch, getState) => {
		let tabDef = getState().getIn(['tabDefs', tabName]);
		let loader = tabLoaders[tabDef.get('type')];
		if (loader) {
			return loader(tabName, getState, dispatch);
		}
	};
}
