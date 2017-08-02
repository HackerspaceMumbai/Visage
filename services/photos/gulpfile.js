
var gulp = require('gulp');
var mocha = require('gulp-mocha');
var util = require('gulp-util');
 
gulp.task('test', function () {
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