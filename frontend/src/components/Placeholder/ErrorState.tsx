export const ErrorState = (props: React.PropsWithChildren) => {
  const { children, ...otherProps } = props
  return (
    <div role="alert" {...otherProps}>
      {children}
    </div>
  )
}
