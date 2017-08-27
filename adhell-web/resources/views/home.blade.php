@extends('layouts.app')

@section('content')
	<ul class="nav nav-tabs" id="myTab" role="tablist">
		<li class="nav-item">
			<a class="nav-link active" id="home-tab" data-toggle="tab" href="#home" role="tab" aria-controls="home" aria-expanded="true">Home</a>
		</li>
		<li class="nav-item">
			<a class="nav-link" id="rebuild-tab" data-toggle="tab" href="#rebuild" role="tab" aria-controls="rebuild">Rebuild</a>
		</li>
	</ul>
	<div class="tab-content" id="myTabContent">
		<div class="tab-pane fade show active" id="home" role="tabpanel" aria-labelledby="home-tab">
			<p class="mt-2">You can download the latest build of Adhell here:</p>
			<p><a href="{{ url('/getbuild') }}" class="btn btn-primary btn-lg">Download</a></p>
		</div>
		<div class="tab-pane fade" id="rebuild" role="tabpanel" aria-labelledby="rebuild-tab">
			<p class="mt-2">Currently, there is no status indicator.</p>
			<p>The button works every 2 minutes.</p>
			<p><a href="{{ url('/rebuild') }}" class="btn btn-light btn-lg">Rebuild</a></p>
		</div>
	</div>
@endsection