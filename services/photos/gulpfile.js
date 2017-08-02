
var gulp = require('gulp');
var mocha = require('gulp-mocha');
var util = require('gulp-util');
var argv = require('yargs').argv;
 
gulp.task('test', function () {
  if(argv.FACE_API_KEY != undefined)
    {
        process.env.FACE_API_KEY = argv.FACE_API_KEY;
    }
  return gulp
  .src('test/**/*.js', {read: false})
  .pipe(mocha())
  .once('end', function () {
     process.exit();
      });
});

gulp.task('default', ['test'], function() {
  gulp.watch(['test/*.js'], function() {
    gulp.run('test');
  });
});