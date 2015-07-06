##TODO LIST

* Add ILogger into mix.  Default implementation a trace.
* Transfer short-hand transforms
* Transfer non-short-hand transforms
* Retrofit Cfg-Net's XML parser for default (portable) fromxml method.
* Reference Jint (portable) as default javascript method.


##NOTES
* parsers and script runners should be interchangeable, or confirm to a common interface, and have default implementations.
* everything is build with `entity` pipeline in mind, but if entity is null, it is known to fall back to `process` pipeline
