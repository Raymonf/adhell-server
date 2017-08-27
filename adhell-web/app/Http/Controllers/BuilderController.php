<?php

namespace App\Http\Controllers;

use Illuminate\Http\Request;

class BuilderController extends Controller
{
	protected $url = 'http://127.0.0.1:54236';

	public function showHome()
	{
		return view('home');
	}

	public function getLatest()
	{
		$outFilename = "Adhell.apk";

		// Get filename
		$curl = curl_init($this->url . '/getlatest');
		curl_setopt($curl, CURLOPT_HEADER, true);
		curl_setopt($curl, CURLOPT_RETURNTRANSFER, true);
		curl_setopt($curl, CURLOPT_CUSTOMREQUEST, 'HEAD');
		if (($response = curl_exec($curl)) !== false)
		{
			if (curl_getinfo($curl, CURLINFO_HTTP_CODE) == '200')
			{
				$reDispo = '/^Content-Disposition: .*?filename=(?<f>[^\s]+|\x22[^\x22]+\x22)\x3B?.*$/m';
				if (preg_match($reDispo, $response, $mDispo))
				{
					$outFilename = trim($mDispo['f'],' ";');
				}
			}
		}
		curl_close($curl);

		return response(file_get_contents($this->url . '/getlatest'))
			->header('Content-Type', 'application/octet-stream')
			->header('Content-Transfer-Encoding', 'binary')
			->header('Content-Disposition', 'attachment; filename="' . $outFilename . '"');
	}

	public function rebuildApk()
	{
		$ch = curl_init($this->url . '/newbuild'); 

        curl_setopt($ch, CURLOPT_RETURNTRANSFER, 1);

        $output = curl_exec($ch);

        curl_close($ch);

		if($output == 'notoldenough')
		{
			return 'It has not yet been 2 minutes.';
		}
		else if($output == 'alreadybuilding')
		{
			return 'Adhell is already rebuilding.';
		}
		else if($output == 'working')
		{
			return back();
		}
	}
}
