angular.module('umbraco')
    .controller('FormStorage.FormStorageController',
		function ($scope,assetsService,editorState) {
			$('#loadingLabel').hide();
			$.ajax({
				type: 'GET',
				url: '/Umbraco/Surface/FormStorage/GetFieldsForAlias/?alias=' + escape($scope.model.config.alias),
				datatype: 'json',
				contentType: 'application/json; charset=utf-8',
				success: function (result) {
					var fields = [
									{ name: 'submissionID', title: 'Submission ID', type: 'text', visible: false },
									{
										name: 'datetime', title: 'Date/Time', type: 'date', width: 80, align: 'center',
										itemTemplate: function (value) {
											moment.locale('en');
											var displayDate = moment(parseInt(value.substr(6)));
											return $('<div>').append(displayDate.format('lll'));
										}
									},
									{ name: 'IP', title: 'IP', type: 'text', width: 50, align: 'center' }
								];
					fields = fields.concat(result);
					fields.push({ type: 'control',
								  itemTemplate: function (value, item) {
													var $result = $([]);
													$result = $result.add(this._createDeleteButton(item));
													return $result;
												}
								});
					setupSubmissionsGrid($scope.model.config.alias, $scope.model.config.gridHeight, $scope.model.config.recordsPerPage, fields);
					$('#downloadButton').click(function() {
						var filterString = '';
						var filterObject = $('#submissionsGrid').jsGrid('getFilter');
						$.each(filterObject, function(key, value) {
							if (value != '') {
								filterString += '&' + key + '=' + value;
							}
                        });

                        if ($('#periodFilter').val() != '') {
                            filterString += '&datetime=&period=' + $('#periodFilter').val();
                        }

						var sortingString = '';
						var sortingObject = $('#submissionsGrid').jsGrid('getSorting');
						$.each(sortingObject, function(key, value) {
							if ((key == "field") && (typeof value != 'undefined')) {
								sortingString += '&sortField' + '=' + value;
							}
							else
							if ((key == "order") && (typeof value != 'undefined')) {
								sortingString += '&sortOrder' + '=' + value;
							}
						});
						var filename = '&filename=' + escape(editorState.current.name);
						window.location = '/Umbraco/Surface/FormStorage/DownloadFormSubmissions/?alias=' + escape($scope.model.config.alias) + filename + filterString + sortingString;
					});
					$('#periodFilter').change(function() {
						var filterObject = $('#submissionsGrid').jsGrid('getFilter');
						$("#submissionsGrid").jsGrid("loadData", filterObject);
					});
				},
				error: function () {
					$('#submissionsGrid').html('<p>There is an error with the configuration of your submissions grid.</p>');
				}
			});
		}
	);
	
function setupSubmissionsGrid(alias, gridHeight, recordsPerPage, fields) {
	
	var gridHeightSetting = '300px';
	if (gridHeight != null) {
		gridHeightSetting = gridHeight + 'px';
	}
	var pageSizeSetting = 5;
	if (recordsPerPage != null) {
		pageSizeSetting = recordsPerPage;
	}
	
	$('#submissionsGrid').jsGrid({
		width: '100%',
		height: gridHeightSetting,

		autoload: true,
		editing: false,
		filtering: true,
		inserting: false,
        paging: true,
        pageLoading: true,
        pageSize: pageSizeSetting,
        sorting: true,
		
		confirmDeleting: true,
		deleteConfirm: 'Are you sure you wish to remove this record? This action cannot be undone.',

		controller: {

            loadData: function (filter) {
                if ($('#periodFilter').val() != '') {
                    filter['period'] = $('#periodFilter').val();
                }
				return $.ajax({
					type: 'GET',
					url: '/Umbraco/Surface/FormStorage/FetchFormSubmissionData/?alias=' + escape(alias),
					data: filter,
					dataType: 'json'
				});
			},

			deleteItem: function (item) {
				var d = $.Deferred();
				$.ajax({
					type: 'DELETE',
					url: '/Umbraco/Surface/FormStorage/DeleteFormSubmissionRecord/?submissionID=' + item.submissionID,
					dataType: 'json'
				}).done(function(response) {
					if (response.success === true) {
						d.resolve();
					} else {
						d.reject();
					}
				});
				return d.promise();
			}

		},

		fields: fields
	});
}