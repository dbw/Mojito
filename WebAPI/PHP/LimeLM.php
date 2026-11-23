<?php

/**
* Returns XML format for all functions. See: https://wyday.com/limelm/help/api/response.rest/
*/
define("LM_XML", 1);

/**
* Returns JSON format for all functions. See: https://wyday.com/limelm/help/api/response.json/
*/
define("LM_JSON", 2);

class LimeLM
{
	private static $api_key;
	private static $request;
	private static $responseType;

	/**
	* Set the API key and the response type you want from LimeLM's servers.
	*
	* @param $api_key Your API key to accessing LimeLM. You can find this on your settings page: https://wyday.com/limelm/settings/
	* @param $responseType The response type you want from all the web API functions. Valid options are LM_JSON or LM_XML.
	*/
	public static function SetAPIKey($api_key, $responseType = LM_XML)
	{
		self::$api_key = $api_key;
		self::$responseType = $responseType;

		//NOTE: If you're using the self-hosted version of LimeLM (that is,
		//      LimeLM running on your own servers), then replace the URL with your own.

		// Almost all users should leave this line unchanged.
		self::$request = curl_init('https://wyday.com/limelm/api/rest/');
	}

	/**
	* Cleanup memory if you're not making any more LimeLM requests during the execution of this script.
	*/
	public static function CleanUp()
	{
		// close curl object
		curl_close(self::$request);
	}

	/**
	* Add a new license feature for a particular version. Note that if you want to set the feature values for existing product keys, use the SetDetails() function.
	*
	* @param $version_id The version ID to add the new feature in.
	* @param $name The name of the feature.
	* @param $required Whether this feature will be required to be filled out when a new product key is created.
	* @param $type The type of the feature. Possible values are "string", "date_time", "int"
	* @param $ta_readable Whether this license field will be readable from your application using the TurboActivate function GetFeatureValue().
	* @param $description The description of the license field that will appear when you create and edit product keys. Maximum 255 characters.
	* @throws Exception If you fail to specify an API key.
	* @return string XML or JSON response from LimeLM.
	*/
	public static function AddFeature($version_id, $name, $required, $type = null, $ta_readable = null, $description = null, $default_val = null, $default_dt_relative = null)
	{
		$post_data = [
			'method' => 'limelm.feature.add',
			'version_id' => $version_id,
			'name' => $name,
			'required' => $required ? 'true' : 'false'
		];

		if ($type)
			$post_data['type'] = $type;

		if ($ta_readable !== null)
			$post_data['ta_readable'] = $ta_readable ? 'true' : 'false';

		if ($description !== null)
			$post_data['description'] = $description;

		if ($default_val !== null)
		{
			$post_data['default_val'] = $default_val;
			$post_data['default_dt_relative'] = $default_dt_relative;
		}

		return self::PostRequest($post_data);
	}

	/**
	* Delete an existing license feature. If you delete a feature it is removed from all the product keys.
	*
	* @param $feature_id The id of the feature you'll be deleting.
	* @throws Exception If you fail to specify an API key.
	* @return string XML or JSON response from LimeLM.
	*/
	public static function DeleteFeature($feature_id)
	{
		$post_data = [
			'method' => 'limelm.feature.delete',
			'feature_id' => $feature_id
		];

		return self::PostRequest($post_data);
	}

	/**
	* Edit an existing license feature. Note that if you want to set the feature values for existing product keys, use the SetDetails() function.
	*
	* @param $feature_id The id of the feature you'll be editing.
	* @param $name The new name of the feature.
	* @param $required Whether this feature will be required to be filled out when a new product key is created.
	* @param $ta_readable Whether this license field will be readable from your application using the TurboActivate function GetFeatureValue().
	* @param $description The description of the license field that will appear when you create and edit product keys. Maximum 255 characters.
	* @throws Exception If you fail to specify an API key.
	* @return string XML or JSON response from LimeLM.
	*/
	public static function EditFeature($feature_id, $name = null, $required = null, $ta_readable = null, $description = null, $default_val = null, $default_dt_relative = null)
	{
		$post_data = [
			'method' => 'limelm.feature.edit',
			'feature_id' => $feature_id
		];

		if ($name)
			$post_data['name'] = $name;

		if ($required !== null)
			$post_data['required'] = $required ? 'true' : 'false';

		if ($ta_readable !== null)
			$post_data['ta_readable'] = $ta_readable ? 'true' : 'false';

		if ($description !== null)
			$post_data['description'] = $description;

		if ($default_val !== null)
		{
			$post_data['default_val'] = $default_val;
			$post_data['default_dt_relative'] = $default_dt_relative;
		}

		return self::PostRequest($post_data);
	}

	/**
	* Get all custom license fields for a particular version.
	*
	* @param $version_id The id of the version to get all the custom license fields for.
	* @throws Exception If you fail to specify an API key.
	* @return string XML or JSON response from LimeLM.
	*/
	public static function GetAllFeatures($version_id)
	{
		$post_data = [
			'method' => 'limelm.feature.getAll',
			'version_id' => $version_id
		];

		return self::PostRequest($post_data);
	}

	/**
	* Replace the tags of the product key with the new set of tags.
	*
	* @param $pkey_id The ID for the product key. Use GetPKeyID to get the pkey ID from a product key.
	* @param $tags One or more tags to associate with the product keys.
	* @throws Exception If you fail to specify an API key.
	* @return string XML or JSON response from LimeLM.
	*/
	public static function SetPKeyTags($pkey_id, $tags)
	{
		$post_data = [
			'method' => 'limelm.pkey.setTags',
			'pkey_id' => $pkey_id,
			'tag' => $tags
		];

		return self::PostRequest($post_data);
	}

	/**
	* Remove a single tag from a product key.
	*
	* @param $pkey_id The ID for the product key. Use GetPKeyID to get the pkey ID from a product key.
	* @param $tag The tag to remove from the product key.
	* @throws Exception If you fail to specify an API key.
	* @return string XML or JSON response from LimeLM.
	*/
	public static function RemovePKeyTag($pkey_id, $tag)
	{
		$post_data = [
			'method' => 'limelm.pkey.removeTag',
			'pkey_id' => $pkey_id,
			'tag' => $tag
		];

		return self::PostRequest($post_data);
	}

	/**
	* Remove a tag from all product keys.
	*
	* @param $tag The tag to remove from all product keys.
	* @throws Exception If you fail to specify an API key.
	* @return string XML or JSON response from LimeLM.
	*/
	public static function DeleteTag($tag)
	{
		$post_data = [
			'method' => 'limelm.tag.delete',
			'tag' => $tag
		];

		return self::PostRequest($post_data);
	}

	/**
	* Gets the activity of a product version between a date range.
	*
	* @param $version_id The id of the version for which you want to get activity.
	* @param $start The start of the date range in the form YYYY-MM-DD (e.g. 2010-06-25) or YYYY-MM-DD hh:mm:ss.
	* @param $end The end of the date range in the form YYYY-MM-DD (e.g. 2011-06-25) or YYYY-MM-DD hh:mm:ss. If this value is not present it finds all activity up until the present time.
	* @param $created Whether or not to get newly created product keys.
	* @param $revoked Whether or not to get recent revoked & unrevoked product keys.
	* @param $act Whether or not to get recent activations for product keys.
	* @param $deact Whether or not to get recent deactivations for product keys.
	* @throws Exception If you fail to specify an API key.
	* @return string XML or JSON response from LimeLM.
	*/
	public static function PKeyActivity($version_id, $start, $end, $created = true, $revoked = false, $act = false, $deact = false)
	{
		$post_data = [
			'method' => 'limelm.pkey.activity',
			'version_id' => $version_id,
			'start' => $start,
			'end' => $end,
			'created' => $created ? 'true' : 'false',
			'revoked' => $revoked ? 'true' : 'false',
			'activation' => $act ? 'true' : 'false',
			'deactivation' => $deact ? 'true' : 'false'
		];

		return self::PostRequest($post_data);
	}

	public static function Deactivate($act_id)
	{
		$post_data = [
			'method' => 'limelm.pkey.deactivate',
			'act_id' => $act_id
		];

		return self::PostRequest($post_data);
	}

	public static function DeletePKey($pkey_id)
	{
		$post_data = [
			'method' => 'limelm.pkey.delete',
			'pkey_id' => $pkey_id
		];

		return self::PostRequest($post_data);
	}

	/**
	* Find all the product keys based on search parameters.
	*
	* @param $num The number of product keys to return per page.
	* @param $page The page you're currently on.
	* @param $product_id The id of the product to search for the product key. You can get the product ID by examining the URL in your browser. For instance, from the URL https://wyday.com/limelm/product/view/101/ you can see the product ID is 101.

If the product_id parameter is null then this function will search all products in your LimeLM account (unless a version_id parameter is specified).
	* @param $version_id The id of the version to search for the product key. You can get the version ID by examining the URL in your browser. For instance, from the URL http://wyday.com/limelm/version/100/ you can see the version ID is 100. (Note: The Version ID is not the Version GUID).

If the version_id parameter is null then this function will search all versions in your LimeLM account (unless a product_id parameter is specified).
	* @param $email The email associated with the product key(s). If you set this to null then this function will ignore the email field. You can also use the asterisk wildcard character "*" to do partial matches of emails.
	* @param $start The start of the date range in the form YYYY-MM-DD (e.g. 2010-06-25) or YYYY-MM-DD hh:mm:ss. If this parameter is null it finds all product keys created since the beginning of time.
	* @param $end The end of the date range in the form YYYY-MM-DD (e.g. 2011-06-25) or YYYY-MM-DD hh:mm:ss. If this parameter is null it finds all activity up until the present time.
	* @param $feature_names Search for product keys with any of the provided license fields or, if feature_use_or is false, search for product keys that have all of the provided license fields. You must pass an array of fields like array('field1', 'field2', ...). You must use the parameter in tandem with "feature_value" and "feature_match".
	* @param $feature_values 
	* @param $feature_match 
	* @param $feature_use_or 

	* @param $tags Search for product keys with either any of the provided tags or, if tag_use_or is false, search for product keys that have all of the provided tags. You can also use the asterisk wildcard character "*" to do partial matches of tags.
	* @param $tag_use_or 
	* @throws Exception If you fail to specify an API key.
	* @return string XML or JSON response from LimeLM.
	*/
	public static function AdvancedPKeySearch($num = 20, $page = 1, $product_id = null, $version_id = null, $email = null, $start = null, $end = null, $feature_names = null, $feature_values = null, $feature_match = null, $feature_use_or = true, $tags = null, $tag_use_or = true, $get_acts = false, $get_features = false, $get_tags = false)
	{
		$post_data = [
			'method' => 'limelm.pkey.advancedSearch',
			'num' => $num,
			'page' => $page
		];

		if ($product_id)
			$post_data['product_id'] = $product_id;

		if ($version_id)
			$post_data['version_id'] = $version_id;

		if ($email)
			$post_data['email'] = $email;

		if ($start)
			$post_data['start'] = $start;

		if ($end)
			$post_data['end'] = $end;

		if ($feature_names)
		{
			$post_data['feature_name'] = $feature_names;
			$post_data['feature_value'] = $feature_values;
			$post_data['feature_match'] = $feature_match;

			if (!$feature_use_or)
				$post_data['feature_use_or'] = 'false';
		}

		if ($tags)
		{
			$post_data['tag'] = $tags;

			if (!$tag_use_or)
				$post_data['tag_use_or'] = 'false';
		}

		if ($get_acts)
			$post_data['get_acts'] = 'true';

		if ($get_features)
			$post_data['get_features'] = 'true';

		if ($get_tags)
			$post_data['get_tags'] = 'true';

		return self::PostRequest($post_data);
	}

	/**
	* Finds the product keys matching an email address. See: https://wyday.com/limelm/help/api/limelm.pkey.find/
	*
	* @param $version_id The version ID to search for the product keys. You can get the version ID by examining the URL in your browser. For instance, from the URL http://wyday.com/limelm/version/100/ you can see the version ID is 100. (Note: The Version ID is not the Version GUID).
	* @param $email The email associated with the product keys.
	* @throws Exception If you fail to specify an API key.
	* @return string XML or JSON response from LimeLM.
	*/
	public static function FindPKey($version_id, $email)
	{
		$post_data = [
			'method' => 'limelm.pkey.find',
			'version_id' => $version_id,
			'email' => $email
		];

		return self::PostRequest($post_data);
	}

	/**
	* Generate product keys. See: https://wyday.com/limelm/help/api/limelm.pkey.generate/
	*
	* @param $version_id The version ID to generate the product keys in. You can get the version ID by examining the URL in your browser. For instance, from the URL http://wyday.com/limelm/version/100/ you can see the version ID is 100. (Note: The Version ID is not the Version GUID).
	* @param $num_keys The number of product keys to generate.
	* @param $num_acts The number of activations per product key.
	* @param $email The email to associate with the product keys. Set to NULL for no email.
	* @param $feature_names A string array list of feature names to set. Set to null if you don't want to set any features.
	* @param $feature_values A string array list of feature values to set. Set to null if you don't want to set any features.
	* @param $tags A string array list of tags to set. Set to null if you don't want to set any tags.
	* @param $for_tfs Whether the keys are for the TurboFloat Server.
	* @param $allow_vm Whether to allow virtual machine activations.
	* @param $deact_limit The limit of the number deactivations that can happen on the product key (-2 inherit from the version, -1 unlimited deactivations, 0 through 9223372036854775807: Limited to the specified number of deactivations.)
	* @param $uname_limit_names A string array list of usernames to limit the TurboFloat Server activated with this key to only allow requests from the specified usernames. <a href="https://wyday.com/limelm/help/licensing-types/#named-user">Read more about named-user / per-seat licensing here</a>.
	* @param $uname_limit_platforms A string array list of the platforms that the usernames will be allowed on. The number of platforms must match the number of usernames provided in the <code>uname_limit_name[]</code> parameter.
			<ul>
			<li><strong><code>0</code></strong>: The username will work from any platform.</li>
			<li><strong><code>1</code></strong>: The username will only work from Windows machines.</li>
			<li><strong><code>2</code></strong>: The username will only work from Mac OS X machines.</li>
			<li><strong><code>3</code></strong>: The username will only work from Linux machines.</li>
			</ul>
	* @throws Exception If you fail to specify an API key.
	* @return string XML or JSON response from LimeLM.
	*/
	public static function GeneratePKeys($version_id, $num_keys = 1, $num_acts = 1, $email = null, $feature_names = null, $feature_values = null, $tags = null, $for_tfs = false, $allow_vm = null, $deact_limit = null, $uname_limit_names = null, $uname_limit_platforms = null)
	{
		$post_data = [
			'method' => 'limelm.pkey.generate',
			'version_id' => $version_id,
			'num_keys' => $num_keys,
			'num_acts' => $num_acts
		];

		if ($email)
			$post_data['email'] = $email;

		if ($feature_names)
		{
			if (!is_array($feature_names) || !is_array($feature_values))
				throw new Exception('$feature_names and $feature_values must be arrays.');

			$post_data['feature_name'] = $feature_names;
			$post_data['feature_value'] = $feature_values;
		}

		if ($tags)
			$post_data['tag'] = $tags;

		if ($for_tfs)
			$post_data['for_tfs'] = 'true';

		if ($allow_vm !== null)
			$post_data['allow_vm'] = $allow_vm;

		if ($deact_limit !== null)
			$post_data['deact_limit'] = $deact_limit;

		if ($uname_limit_names !== null)
		{
			if (!is_array($uname_limit_names) || !is_array($uname_limit_platforms))
				throw new Exception('$uname_limit_names and $uname_limit_platforms must be arrays.');

			$post_data['uname_limit_name'] = $uname_limit_names;
			$post_data['uname_limit_platform'] = $uname_limit_platforms;
		}

		return self::PostRequest($post_data);
	}

	/**
	* Gets the details for a product key.
	*
	* @param $pkey_id The ID of the product key.
	* @throws Exception If you fail to specify an API key.
	* @return string XML or JSON response from LimeLM.
	*/
	public static function GetPKeyDetails($pkey_id)
	{
		$post_data = [
			'method' => 'limelm.pkey.getDetails',
			'pkey_id' => $pkey_id
		];

		return self::PostRequest($post_data);
	}

	/**
	* Get the ID for a product key.
	*
	* @param string $pkey The product key.
	* @throws Exception If you fail to specify an API key.
	* @return string XML or JSON response from LimeLM.
	*/
	public static function GetPKeyID($pkey)
	{
		$post_data = [
			'method' => 'limelm.pkey.getID',
			'pkey' => $pkey
		];

		return self::PostRequest($post_data);
	}

	/**
	* Manually activates using the contents of an activation request file.
	*
	* @param $xml_act_request The contents of the XML activation request file.
	* @throws Exception If you fail to specify an API key.
	* @return string XML or JSON response from LimeLM.
	*/
	public static function ManualActivation($xml_act_request)
	{
		$post_data = [
			'method' => 'limelm.pkey.manualActivation',
			'act_req_xml' => $xml_act_request
		];

		return self::PostRequest($post_data);
	}

	/**
	* Manually deactivates using the contents of a deactivation request file.
	*
	* @param $xml_deact_request The contents of the XML deactivation request file.
	* @throws Exception If you fail to specify an API key.
	* @return string XML or JSON response from LimeLM.
	*/
	public static function ManualDeactivation($xml_deact_request)
	{
		$post_data = [
			'method' => 'limelm.pkey.manualDeactivation',
			'deact_req_xml' => $xml_deact_request
		];

		return self::PostRequest($post_data);
	}

	/**
	* Revoke or unrevoke a product key.
	*
	* @param $pkey_id The ID of the product key.
	* @param $revoke Whether to revoke or unrevoke product keys.
	* @throws Exception If you fail to specify an API key.
	* @return string XML or JSON response from LimeLM.
	*/
	public static function RevokePKey($pkey_id, $revoke = true)
	{
		$post_data = [
			'method' => 'limelm.pkey.revoke',
			'pkey_id' => $pkey_id,
			'revoke' => $revoke ? 'true' : 'false'
		];

		return self::PostRequest($post_data);
	}

	/**
	* Sets the details for a product key. This include the number of activations, email, and any license features.
	*
	* @param $pkey_id The ID of the product key.
	* @param $num_acts If non-zero this changes the number of activations allowed for the product key.
	* @param $email If this is non-null this changes the email address associated with the product key.
	* @param $feature_names A string array list of feature names to set. Set to null if you don't want to set any features.
	* @param $feature_values A string array list of feature values to set. Set to null if you don't want to set any features.
	* @param $for_tfs Whether the keys are for the TurboFloat Server.
	* @param $allow_vm Whether to allow virtual machine activations.
	* @param $deact_limit The limit of the number deactivations that can happen on the product key (-2 inherit from the version, -1 unlimited deactivations, 0 through 9223372036854775807: Limited to the specified number of deactivations.)
	* @throws Exception If you fail to specify an API key.
	* @return string XML or JSON response from LimeLM.
	*/
	public static function SetPKeyDetails($pkey_id, $num_acts = null, $email = null, $feature_names = null, $feature_values = null, $for_tfs = null, $allow_vm = null, $deact_limit = null, $uname_limit_names = null, $uname_limit_platforms = null)
	{
		$post_data = [
			'method' => 'limelm.pkey.setDetails',
			'pkey_id' => $pkey_id
		];

		if ($num_acts !== null)
			$post_data['num_acts'] = $num_acts;

		if ($email !== null)
			$post_data['email'] = $email;

		if ($feature_names !== null)
		{
			if (!is_array($feature_names) || !is_array($feature_values))
				throw new Exception('$feature_names and $feature_values must be arrays.');

			$post_data['feature_name'] = $feature_names;
			$post_data['feature_value'] = $feature_values;
		}

		if ($for_tfs !== null)
			$post_data['for_tfs'] = $for_tfs ? 'true' : 'false';

		if ($allow_vm !== null)
			$post_data['allow_vm'] = $allow_vm;

		if ($deact_limit !== null)
			$post_data['deact_limit'] = $deact_limit;

		if ($uname_limit_names !== null)
		{
			if (!is_array($uname_limit_names) || !is_array($uname_limit_platforms))
				throw new Exception('$uname_limit_names and $uname_limit_platforms must be arrays.');

			$post_data['uname_limit_name'] = $uname_limit_names;
			$post_data['uname_limit_platform'] = $uname_limit_platforms;
		}

		return self::PostRequest($post_data);
	}

	public static function DeleteTrialExtension($extension_id)
	{
		$post_data = [
			'method' => 'limelm.trialExtension.delete',
			'ext_id' => $extension_id
		];

		return self::PostRequest($post_data);
	}

	public static function GetTrialExtensionDetails($extension_id)
	{
		$post_data = [
			'method' => 'limelm.trialExtension.getDetails',
			'ext_id' => $extension_id
		];

		return self::PostRequest($post_data);
	}

	public static function GetTrialExtensionID($version_id, $ext)
	{
		$post_data = [
			'method' => 'limelm.trialExtension.getID',
			'version_id' => $version_id,
			'ext' => $ext
		];

		return self::PostRequest($post_data);
	}

	public static function TrialExtensionSearch($version_id, $customer_id, $fulldets = false)
	{
		$post_data = [
			'method' => 'limelm.trialExtension.search',
			'version_id' => $version_id,
			'customer_id' => $customer_id
		];

		return self::PostRequest($post_data);
	}

	/**
	* Generates a trial extension for a version ID.
	*
	* @param $version_id The version ID to generate the trial extension in.
	* @param $is_online Is the trial extension a small online extension or the larger offline extension.
	* @param $length The length of the trial extension in days.
	* @param $expires When the trial expires in the form YYYY-MM-DD
	* @param $customer_id An optional customer ID (e.g. email) to associate with the trial extension.
	* @param $max_uses The maximum number of uses for the online trial extension.
	* @throws Exception If you fail to specify an API key.
	* @return string XML or JSON response from LimeLM.
	*/
	public static function GenerateTrialExtension($version_id, $is_online, $length, $expires, $customer_id = null, $max_uses = null)
	{
		$post_data = [
			'method' => 'limelm.trialExtension.generate',
			'version_id' => $version_id,
			'is_online' => $is_online ? 'true' : 'false',
			'length' => $length,
			'expires' => $expires
		];

		if ($is_online)
			$post_data['max_uses'] = $max_uses;

		if ($customer_id !== null)
			$post_data['customer_id'] = $customer_id;

		return self::PostRequest($post_data);
	}

	/**
	 * Find all the verified trials based on search parameters.
	 *
	 * @param $num The number of product keys to return per page.
	 * @param $page The page you're currently on.
	 * @param $product_id The id of the product to search for the product key. You can get the product ID by examining the URL in your browser. For instance, from the URL https://wyday.com/limelm/product/view/101/ you can see the product ID is 101.
	 *
	 * If the product_id parameter is null then this function will search all products in your LimeLM account (unless a version_id parameter is specified).
	 * 
	 * @param $version_id The id of the version to search for the product key. You can get the version ID by examining the URL in your browser. For instance, from the URL http://wyday.com/limelm/version/100/ you can see the version ID is 100. (Note: The Version ID is not the Version GUID).
	 *
	 * If the version_id parameter is null then this function will search all versions in your LimeLM account (unless a product_id parameter is specified).
	 * 
	 * @param $extra_data The extra data associated with the verified trial. If you set this to null then this function will ignore the extra_data field. You can also use the asterisk wildcard character "*" to do partial matches of extra data.
	 * @param $create_start The start of the date range in the form YYYY-MM-DD (e.g. 2010-06-25) or YYYY-MM-DD hh:mm:ss. If this parameter is null it finds all verified trials created since the beginning of time.
	 * @param $create_end The end of the date range in the form YYYY-MM-DD (e.g. 2011-06-25) or YYYY-MM-DD hh:mm:ss. If this parameter is null it finds all activity up until the present time.
	 * @throws Exception If you fail to specify an API key.
	 * @return string XML or JSON response from LimeLM.
	 */
	public static function AdvancedTrialSearch($num = 20, $page = 1, $product_id = null, $version_id = null, $extra_data = null, $create_start = null, $create_end = null)
	{
		$post_data = [
			'method' => 'limelm.trial.advancedSearch',
			'num' => $num,
			'page' => $page
		];

		if ($product_id)
			$post_data['product_id'] = $product_id;

		if ($version_id)
			$post_data['version_id'] = $version_id;

		if ($extra_data)
			$post_data['extra_data'] = $extra_data;

		if ($create_start)
			$post_data['create_start'] = $create_start;

		if ($create_end)
			$post_data['create_end'] = $create_end;

		return self::PostRequest($post_data);
	}


	/**
	* A simple test to confirm your API key is correct.
	*
	* @param $params Array of parameters to echo.
	* @throws Exception If you fail to specify an API key.
	* @return string XML or JSON response from LimeLM.
	*/
	public static function TestEcho($params)
	{
		$params['method'] = 'limelm.test.echo';
		return self::PostRequest($params);
	}

	private static function PostRequest($post_data)
	{
		if (!self::$api_key)
			throw new Exception('You must specify your LimeLM API key and set it using SetAPIKey().');

		$post_data['api_key'] = self::$api_key;

		if (self::$responseType === LM_JSON)
		{
			$post_data['format'] = 'json';
			$post_data['nojsoncallback'] = '1';
		}

		// This section takes the input fields and converts them to the proper format
		// for an http post.  For example: "method=limelm.pkey.find&version_id=100"
		$post_string = '';
		foreach ($post_data as $key => $value)
		{
			if (is_array($value))
			{
				foreach ($value as $sub_value)
					$post_string .= $key.'[]='.urlencode($sub_value).'&';
			}
			else
				$post_string .= $key.'='.urlencode($value).'&';
		}

		$post_string = rtrim($post_string, '& ');

		curl_setopt(self::$request, CURLOPT_HEADER, 0); // eliminate header info from response
		curl_setopt(self::$request, CURLOPT_ENCODING, ""); // support gzip & deflate responses if available
		curl_setopt(self::$request, CURLOPT_RETURNTRANSFER, 1); // Returns response data instead of TRUE(1)
		curl_setopt(self::$request, CURLOPT_POSTFIELDS, $post_string); // use HTTP POST to send form data
		curl_setopt(self::$request, CURLOPT_TCP_NODELAY, TRUE); // disable TCP nagle algorithm

		return curl_exec(self::$request); // execute curl post and store results in $post_response
	}
}